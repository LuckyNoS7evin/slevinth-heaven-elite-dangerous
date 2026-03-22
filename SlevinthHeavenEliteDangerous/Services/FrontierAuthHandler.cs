using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Attaches the Frontier OAuth access token as a Bearer header to every outgoing API request.
/// </summary>
public class FrontierAuthHandler(FrontierAuthService frontierAuthService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await frontierAuthService.GetValidAccessTokenAsync();
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var version = typeof(FrontierAuthHandler).Assembly.GetName().Version;
        if (version is not null)
            request.Headers.TryAddWithoutValidation("X-App-Version",
                $"{version.Major}.{version.Minor}.{version.Build}");

        return await base.SendAsync(request, cancellationToken);
    }
}
