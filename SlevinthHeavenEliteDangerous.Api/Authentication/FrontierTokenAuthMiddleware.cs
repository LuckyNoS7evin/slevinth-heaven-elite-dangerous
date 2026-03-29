using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Api.Authentication;

/// <summary>
/// Validates incoming requests by checking the Bearer token against Frontier's CAPI profile endpoint.
/// Results are cached in-memory for 5 minutes to avoid a CAPI call on every request.
/// Requests to web pages, auth endpoints, and static files are skipped.
/// </summary>
public class FrontierTokenAuthMiddleware(RequestDelegate next, IMemoryCache cache, IHttpClientFactory httpClientFactory)
{
    private const string CapiProfileUrl = "https://companion.orerve.net/profile";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    private record TokenCacheEntry(string CommanderName, string? Fid);

    /// <summary>
    /// Path prefixes that should bypass bearer token validation
    /// (web UI, auth flow, static assets, Blazor framework files).
    /// </summary>
    private static readonly string[] SkipPrefixes =
    [
        "/auth/",
        "/css/",
        "/js/",
        "/favicon",
        "/_framework/",
        "/_blazor",
        "/commander",  // Blazor pages served by Razor components
        "/",           // root page (Home)
    ];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";

        // Skip bearer validation for cookie-authenticated web requests and static assets
        if (ShouldSkip(path, context))
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing or invalid Authorization header. Expected: Bearer <frontier_access_token>");
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        var cacheKey = HashToken(token);

        if (!cache.TryGetValue(cacheKey, out TokenCacheEntry? entry))
        {
            var validated = await ValidateWithFrontierAsync(context, httpClientFactory, token);
            if (!validated.IsValid)
                return; // response already written

            entry = new TokenCacheEntry(validated.CommanderName ?? string.Empty, validated.Fid);
            cache.Set(cacheKey, entry, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTtl,
                Size = 1,
            });
        }

        context.Items["CommanderName"] = entry?.CommanderName;
        context.Items["CommanderFid"] = entry?.Fid;
        await next(context);
    }

    /// <summary>SHA-256 hash of the raw token — never store the token itself as a cache key.</summary>
    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static bool ShouldSkip(string path, HttpContext context)
    {
        // Already authenticated via cookie — let it through
        if (context.User.Identity?.IsAuthenticated == true)
            return true;

        // API endpoints require bearer tokens
        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            return false;

        // Journal/diagnostics/version/weatherforecast endpoints require bearer tokens
        if (path.StartsWith("/journal/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/diagnostics", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/version", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/weatherforecast", StringComparison.OrdinalIgnoreCase))
            return false;

        // Everything else (Blazor pages, static files, auth endpoints) — skip
        return true;
    }

    private static async Task<(bool IsValid, string? CommanderName, string? Fid)> ValidateWithFrontierAsync(
        HttpContext context,
        IHttpClientFactory httpClientFactory,
        string token)
    {
        try
        {
            var http = httpClientFactory.CreateClient("frontier-capi");
            using var request = new HttpRequestMessage(HttpMethod.Get, CapiProfileUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await http.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid or expired Frontier token.");
                return (false, null, null);
            }

            var body = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(body);

            string? commanderName = null;
            string? fid = null;

            if (doc.RootElement.TryGetProperty("commander", out var cmdr))
            {
                if (cmdr.TryGetProperty("name", out var nameProp))
                    commanderName = nameProp.GetString();
                if (cmdr.TryGetProperty("id", out var idProp))
                    fid = idProp.ToString();
            }

            return (true, commanderName, fid);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsync($"Unable to verify token with Frontier: {ex.Message}");
            return (false, null, null);
        }
    }
}
