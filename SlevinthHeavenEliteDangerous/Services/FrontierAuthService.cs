using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Handles Frontier Developments OAuth 2.0 PKCE authentication and Companion API access.
/// </summary>
public sealed class FrontierAuthService : IDisposable
{
    private const string AuthEndpoint  = "https://auth.frontierstore.net/auth";
    private const string TokenEndpoint = "https://auth.frontierstore.net/token";
    private const string CapiBaseUrl   = "https://companion.orerve.net";

    private readonly string _clientId;
    private readonly HttpClient _http = new();
    private readonly FrontierAuthDataService _dataService;
    private FrontierTokens? _tokens;

    public event EventHandler? AuthStateChanged;

    public bool IsAuthenticated => _tokens is not null && !string.IsNullOrEmpty(_tokens.RefreshToken) && !_tokens.IsRefreshTokenExpired;
    public string? CommanderName => _tokens?.CommanderName;

    public FrontierAuthService(ApiConfigService _, FrontierAuthDataService dataService)
    {
        _clientId    = AppResources.FrontierClientId;
        _dataService = dataService;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Loads persisted tokens and attempts a silent access-token refresh on startup.
    /// </summary>
    public async Task TryRestoreSessionAsync()
    {
        _tokens = await _dataService.LoadDataAsync();

        if (_tokens is null || string.IsNullOrEmpty(_tokens.RefreshToken))
            return;

        if (_tokens.IsRefreshTokenExpired)
        {
            Debug.WriteLine("[FrontierAuth] Refresh token expired (>25 days) — user must log in again.");
            _tokens = null;
            return;
        }

        try
        {
            await RefreshTokensAsync(_tokens.RefreshToken);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FrontierAuth] Silent restore failed: {ex.Message}");
            // Keep _tokens so commander name still shows; access token just won't work until retry
        }
    }

    /// <summary>
    /// Starts the PKCE browser-based login flow. Opens the browser, listens on
    /// a random localhost port for the callback, then exchanges the code for tokens.
    /// </summary>
    public async Task LoginAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_clientId) || _clientId == "REPLACE_WITH_YOUR_FRONTIER_CLIENT_ID")
            throw new InvalidOperationException("Frontier client ID is not configured. Register your app at user.frontierstore.net.");

        // Generate PKCE verifier (base64url, no padding)
        var rawVerifier = new byte[32];
        RandomNumberGenerator.Fill(rawVerifier);
        var verifier = Base64UrlEncode(rawVerifier);

        // Challenge = SHA256 of verifier string's ASCII bytes, base64url, no padding
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        var challenge = Base64UrlEncode(hash);

        // Random state for CSRF protection
        var rawState = new byte[8];
        RandomNumberGenerator.Fill(rawState);
        var state = Base64UrlEncode(rawState);

        // Pick a free port and build the redirect URI
        var port = GetFreePort();
        var redirectUri = $"http://localhost:{port}/";

        var authUrl = BuildAuthUrl(challenge, state, redirectUri);

        // Start listener before opening the browser
        using var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();

        Process.Start(new ProcessStartInfo { FileName = authUrl, UseShellExecute = true });

        // Wait for the callback, with cancellation support
        var callbackTask = listener.GetContextAsync();
        var cancelTask   = Task.Delay(Timeout.Infinite, cancellationToken);

        var winner = await Task.WhenAny(callbackTask, cancelTask);
        if (winner == cancelTask)
        {
            listener.Stop();
            cancellationToken.ThrowIfCancellationRequested();
        }

        var context = await callbackTask;
        var code    = context.Request.QueryString["code"];

        // Send success page to browser
        var html = Encoding.UTF8.GetBytes(
            "<html><body style='font-family:sans-serif;text-align:center;margin-top:80px'>" +
            "<h2>Login successful</h2><p>You can close this window.</p></body></html>");
        context.Response.ContentType     = "text/html";
        context.Response.ContentLength64 = html.Length;
        await context.Response.OutputStream.WriteAsync(html, cancellationToken);
        context.Response.Close();

        listener.Stop();

        if (string.IsNullOrEmpty(code))
            throw new InvalidOperationException("No authorisation code received from Frontier.");

        await ExchangeCodeAsync(code, verifier, redirectUri, cancellationToken);
        await FetchCommanderNameAsync(cancellationToken);

        await _dataService.SaveDataAsync(_tokens!);
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears tokens and removes saved data.
    /// </summary>
    public void Logout()
    {
        _tokens = null;
        _ = _dataService.SaveDataAsync(new FrontierTokens());
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Returns a valid access token, refreshing silently if needed.
    /// </summary>
    public async Task<string?> GetValidAccessTokenAsync()
    {
        if (_tokens is null) return null;

        if (_tokens.IsAccessTokenExpired)
        {
            try
            {
                await RefreshTokensAsync(_tokens.RefreshToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FrontierAuth] Token refresh failed: {ex.Message}");
                _tokens = null;
                AuthStateChanged?.Invoke(this, EventArgs.Empty);
                return null;
            }
        }

        return _tokens.AccessToken;
    }

    /// <summary>
    /// Calls the CAPI profile endpoint and returns the raw JSON string.
    /// </summary>
    public async Task<string?> GetCapiProfileAsync()
    {
        var token = await GetValidAccessTokenAsync();
        if (token is null) return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, $"{CapiBaseUrl}/profile");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"[FrontierAuth] CAPI /profile failed: {response.StatusCode}");
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    public void Dispose() => _http.Dispose();

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private string BuildAuthUrl(string challenge, string state, string redirectUri)
    {
        var query = new StringBuilder();
        query.Append(AuthEndpoint);
        query.Append("?response_type=code");
        query.Append("&audience=frontier,steam");
        query.Append($"&client_id={Uri.EscapeDataString(_clientId)}");
        query.Append($"&code_challenge={challenge}");
        query.Append("&code_challenge_method=S256");
        query.Append($"&state={state}");
        query.Append($"&redirect_uri={Uri.EscapeDataString(redirectUri)}");
        query.Append("&scope=auth%20capi");
        return query.ToString();
    }

    private async Task ExchangeCodeAsync(string code, string verifier, string redirectUri, CancellationToken ct)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"]    = "authorization_code",
            ["client_id"]     = _clientId,
            ["code"]          = code,
            ["code_verifier"] = verifier,
            ["redirect_uri"]  = redirectUri,
        };

        _tokens = await PostTokenRequestAsync(form, ct);
        _tokens.RefreshTokenIssuedAt = DateTime.UtcNow;
    }

    private async Task RefreshTokensAsync(string refreshToken)
    {
        var form = new Dictionary<string, string>
        {
            ["grant_type"]    = "refresh_token",
            ["client_id"]     = _clientId,
            ["refresh_token"] = refreshToken,
        };

        var old = _tokens;
        _tokens = await PostTokenRequestAsync(form, CancellationToken.None);

        // Preserve fields that aren't in the token response
        _tokens.RefreshTokenIssuedAt = old?.RefreshTokenIssuedAt ?? DateTime.UtcNow;
        _tokens.CommanderName        = old?.CommanderName;

        await _dataService.SaveDataAsync(_tokens);
        AuthStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task<FrontierTokens> PostTokenRequestAsync(
        Dictionary<string, string> form,
        CancellationToken ct)
    {
        using var content  = new FormUrlEncodedContent(form);
        using var response = await _http.PostAsync(TokenEndpoint, content, ct);

        var body = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Token endpoint returned {response.StatusCode}: {body}");

        var doc  = JsonDocument.Parse(body);
        var root = doc.RootElement;

        return new FrontierTokens
        {
            AccessToken          = root.GetProperty("access_token").GetString()!,
            RefreshToken         = root.GetProperty("refresh_token").GetString()!,
            AccessTokenExpiresAt = DateTime.UtcNow.AddSeconds(root.GetProperty("expires_in").GetInt32()),
        };
    }

    private async Task FetchCommanderNameAsync(CancellationToken ct)
    {
        if (_tokens is null) return;

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{CapiBaseUrl}/profile");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokens.AccessToken);

            var response = await _http.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return;

            var body = await response.Content.ReadAsStringAsync(ct);
            var doc  = JsonDocument.Parse(body);

            if (doc.RootElement.TryGetProperty("commander", out var cmdr) &&
                cmdr.TryGetProperty("name", out var nameProp))
            {
                _tokens.CommanderName = nameProp.GetString();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FrontierAuth] Failed to fetch commander name: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------
    // PKCE helper
    // -------------------------------------------------------------------------

    private static string Base64UrlEncode(byte[] bytes)
        => Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");

    // -------------------------------------------------------------------------
    // Port helper
    // -------------------------------------------------------------------------

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
