using Microsoft.AspNetCore.Mvc;
using SlevinthHeavenEliteDangerous.Api.Storage;
using SlevinthHeavenEliteDangerous.Core.Models;

namespace SlevinthHeavenEliteDangerous.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class VersionController(IConfiguration config, CommanderDataStore commanderStore) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var version = config["AppVersion"];
        if (string.IsNullOrWhiteSpace(version))
            return StatusCode(500, "AppVersion is not configured.");

        return Ok(new VersionInfo
        {
            LatestVersion = version,
            ReleaseNotesUrl = config["ReleaseNotesUrl"],
            DownloadUrl = config["DownloadUrl"],
        });
    }

    /// <summary>
    /// Called by the desktop app on startup to record which app version the commander is running.
    /// Uses the FID directly so the record is found/created reliably.
    /// </summary>
    [HttpPost("report")]
    public async Task<IActionResult> Report([FromForm] string fid)
    {
        if (string.IsNullOrWhiteSpace(fid))
            return BadRequest("FID is required.");

        if (!Request.Headers.TryGetValue("X-App-Version", out var appVersion) ||
            string.IsNullOrWhiteSpace(appVersion))
            return BadRequest("X-App-Version header is required.");

        var data = await commanderStore.GetOrCreateAsync(fid);
        data.LastAppVersion = appVersion.ToString();
        await commanderStore.SaveAsync(data);

        return Ok();
    }
}
