using Microsoft.AspNetCore.Mvc;
using SlevinthHeavenEliteDangerous.Api.Storage;

namespace SlevinthHeavenEliteDangerous.Api.Controllers;

/// <summary>
/// Accepts raw journal file uploads from the desktop app.
/// Files are saved to disk immediately; processing happens in the background
/// via <see cref="Processing.JournalProcessingService"/>.
/// </summary>
[ApiController]
[Route("[controller]")]
public class JournalController(
    JournalFileStore journalStore,
    CommanderDataStore commanderStore,
    ILogger<JournalController> logger) : ControllerBase
{
    /// <summary>
    /// Upload a raw journal file.
    /// The file is saved to disk immediately; processing happens in the background.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(50_000_000)] // 50 MB
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] string fid)
    {
        var commanderName = HttpContext.Items["CommanderName"] as string;

        if (file is null || file.Length == 0)
            return BadRequest("No file provided.");

        if (string.IsNullOrWhiteSpace(fid))
            return BadRequest("FID is required.");

        var fileName = file.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("File name is required.");

        await using var stream = file.OpenReadStream();
        await journalStore.SaveFileAsync(fid, fileName, stream);

        // Record the app version reported by the desktop client
        if (Request.Headers.TryGetValue("X-App-Version", out var appVersion) &&
            !string.IsNullOrWhiteSpace(appVersion))
        {
            var data = await commanderStore.GetOrCreateAsync(fid);
            data.LastAppVersion = appVersion.ToString();
            await commanderStore.SaveAsync(data);
        }

        logger.LogInformation(
            "[Journal] Received {FileName} ({Size:N0} bytes) from CMDR {Commander} (FID {FID})",
            fileName, file.Length, commanderName, fid);

        return Ok(new { fileName, size = file.Length, fid });
    }
}
