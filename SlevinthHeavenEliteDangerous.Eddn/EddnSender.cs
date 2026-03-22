using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SlevinthHeavenEliteDangerous.Eddn;

public sealed record EddnSendResult(bool Success, int StatusCode, string? Error);

/// <summary>
/// Thin HTTP wrapper that POSTs a single message to the EDDN upload endpoint.
/// </summary>
public sealed class EddnSender(
    IHttpClientFactory httpClientFactory,
    IOptions<EddnOptions> options,
    ILogger<EddnSender> logger)
{
    public async Task<EddnSendResult> SendAsync(EddnMessage message, CancellationToken ct)
    {
        var endpoint = options.Value.Endpoint;
        var json = message.ToJson(options.Value.TestMode);

        try
        {
            var client = httpClientFactory.CreateClient("eddn");
            var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Version = HttpVersion.Version11,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            var response = await client.SendAsync(request, ct);

            if (response.IsSuccessStatusCode)
                return new EddnSendResult(true, (int)response.StatusCode, null);

            var error = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("[EDDN] Upload failed {Status}: {Error}", (int)response.StatusCode, error);
            return new EddnSendResult(false, (int)response.StatusCode, error);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[EDDN] Exception sending message");
            return new EddnSendResult(false, 0, ex.Message);
        }
    }
}
