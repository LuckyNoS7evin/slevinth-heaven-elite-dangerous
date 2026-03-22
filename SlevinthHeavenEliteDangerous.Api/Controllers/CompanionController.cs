using Microsoft.AspNetCore.Mvc;
using SlevinthHeavenEliteDangerous.Eddn;

namespace SlevinthHeavenEliteDangerous.Api.Controllers;

/// <summary>
/// Accepts companion file uploads (Market.json, Shipyard.json, Outfitting.json, FCMaterials.json)
/// from the desktop app and forwards them to EDDN.
/// </summary>
[ApiController]
[Route("[controller]")]
public class CompanionController(
    EddnPublisherService eddnPublisher,
    ILogger<CompanionController> logger) : ControllerBase
{
    private static readonly HashSet<string> ValidTypes = new(StringComparer.OrdinalIgnoreCase)
        { "market", "shipyard", "outfitting", "fcmaterials" };

    /// <summary>
    /// Upload a companion file for EDDN forwarding.
    /// </summary>
    /// <param name="type">File type: market, shipyard, outfitting, or fcmaterials.</param>
    /// <param name="fid">Commander FID (used to look up game version for EDDN header).</param>
    /// <param name="file">The companion JSON file.</param>
    [HttpPost("upload")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> Upload(
        [FromForm] string type,
        [FromForm] string fid,
        IFormFile file,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(type) || !ValidTypes.Contains(type))
            return BadRequest("Invalid or missing type. Expected: market, shipyard, outfitting, fcmaterials.");

        if (string.IsNullOrWhiteSpace(fid))
            return BadRequest("FID is required.");

        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        using var reader = new StreamReader(file.OpenReadStream());
        var json = await reader.ReadToEndAsync(ct);

        await eddnPublisher.ProcessCompanionAsync(type, json, fid, ct);

        logger.LogInformation("[Companion] Received {Type} ({Size:N0} bytes) from FID {FID}",
            type, file.Length, fid);

        return Ok(new { type, fid });
    }
}
