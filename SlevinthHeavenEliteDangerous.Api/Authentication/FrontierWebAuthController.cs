using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Api.Authentication;

/// <summary>
/// Handles the Frontier OAuth 2.0 PKCE flow for browser-based login.
/// <list type="bullet">
///   <item><c>GET /auth/login</c> — redirects to Frontier auth</item>
///   <item><c>GET /auth/callback</c> — exchanges code for tokens, creates cookie</item>
///   <item><c>GET /auth/logout</c> — signs out</item>
/// </list>
/// </summary>
[Route("auth")]
public class FrontierWebAuthController(
    IConfiguration config,
    IMemoryCache cache,
    IHttpClientFactory httpClientFactory,
    ILogger<FrontierWebAuthController> logger) : Controller
{
    private const string AuthEndpoint = "https://auth.frontierstore.net/auth";
    private const string TokenEndpoint = "https://auth.frontierstore.net/token";
    private const string CapiProfileUrl = "https://companion.orerve.net/profile";

    [HttpGet("login")]
    public IActionResult Login()
    {
        var clientId = config["FrontierWeb:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            return BadRequest("FrontierWeb:ClientId is not configured.");

        // PKCE verifier
        var rawVerifier = new byte[32];
        RandomNumberGenerator.Fill(rawVerifier);
        var verifier = Base64UrlEncode(rawVerifier);

        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        var challenge = Base64UrlEncode(hash);

        // State for CSRF + cache key
        var rawState = new byte[16];
        RandomNumberGenerator.Fill(rawState);
        var state = Base64UrlEncode(rawState);

        // Store verifier in cache keyed by state (5 min TTL)
        cache.Set($"pkce:{state}", verifier, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            Size = 1,
        });

        var redirectUri = GetRedirectUri();

        var authUrl = $"{AuthEndpoint}" +
            $"?response_type=code" +
            $"&audience=frontier,steam" +
            $"&client_id={Uri.EscapeDataString(clientId)}" +
            $"&code_challenge={challenge}" +
            $"&code_challenge_method=S256" +
            $"&state={state}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            $"&scope=auth%20capi";

        return Redirect(authUrl);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
    {
        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            return BadRequest("Missing code or state.");

        if (!cache.TryGetValue($"pkce:{state}", out string? verifier) || string.IsNullOrEmpty(verifier))
            return BadRequest("Invalid or expired state. Please try logging in again.");

        cache.Remove($"pkce:{state}");

        var clientId = config["FrontierWeb:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            return StatusCode(500, "FrontierWeb:ClientId is not configured.");

        var redirectUri = GetRedirectUri();

        try
        {
            // Exchange code for tokens
            var http = httpClientFactory.CreateClient("frontier-capi");
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = clientId,
                ["code"] = code,
                ["code_verifier"] = verifier,
                ["redirect_uri"] = redirectUri,
            };

            using var tokenResponse = await http.PostAsync(TokenEndpoint, new FormUrlEncodedContent(form));
            var tokenBody = await tokenResponse.Content.ReadAsStringAsync();

            if (!tokenResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("[WebAuth] Token exchange failed: {Status} {Body}", tokenResponse.StatusCode, tokenBody);
                return BadRequest("Failed to exchange authorisation code.");
            }

            var tokenDoc = JsonDocument.Parse(tokenBody);
            var accessToken = tokenDoc.RootElement.GetProperty("access_token").GetString()!;

            // Fetch commander profile
            using var profileRequest = new HttpRequestMessage(HttpMethod.Get, CapiProfileUrl);
            profileRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            using var profileResponse = await http.SendAsync(profileRequest);

            if (!profileResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("[WebAuth] CAPI profile failed: {Status}", profileResponse.StatusCode);
                return BadRequest("Failed to fetch commander profile from Frontier.");
            }

            var profileBody = await profileResponse.Content.ReadAsStringAsync();
            var profileDoc = JsonDocument.Parse(profileBody);

            string? commanderName = null;
            string? fid = null;

            if (profileDoc.RootElement.TryGetProperty("commander", out var cmdr))
            {
                if (cmdr.TryGetProperty("name", out var nameProp))
                    commanderName = nameProp.GetString();
                if (cmdr.TryGetProperty("id", out var idProp))
                    fid = idProp.ToString();
            }

            if (string.IsNullOrEmpty(commanderName))
                return BadRequest("Could not determine commander name.");

            // Create cookie identity
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, commanderName),
            };

            if (!string.IsNullOrEmpty(fid))
                claims.Add(new Claim("FID", fid));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                });

            logger.LogInformation("[WebAuth] CMDR {Commander} logged in via web.", commanderName);

            return Redirect("/");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[WebAuth] OAuth callback failed.");
            return StatusCode(500, "Authentication failed. Please try again.");
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    private string GetRedirectUri()
    {
        var configured = config["FrontierWeb:RedirectUri"];
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        // Auto-derive from the current request
        return $"{Request.Scheme}://{Request.Host}/auth/callback";
    }

    private static string Base64UrlEncode(byte[] input) =>
        Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
