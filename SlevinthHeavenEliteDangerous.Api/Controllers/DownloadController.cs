using Microsoft.AspNetCore.Mvc;

namespace SlevinthHeavenEliteDangerous.Api.Controllers;

/// <summary>
/// Serves the desktop app installer from Data/Downloads/.
/// </summary>
[ApiController]
[Route("[controller]")]
public class DownloadController(IWebHostEnvironment env, IConfiguration config) : ControllerBase
{
    private readonly string _downloadsPath = Path.Combine(env.ContentRootPath, "Data", "Downloads");

    [HttpGet("app")]
    public IActionResult GetApp()
    {
        Directory.CreateDirectory(_downloadsPath);

        var fileName = config["DownloadFileName"] ?? "SlevinthHeavenEliteDangerous-Setup.exe";
        var filePath = Path.Combine(_downloadsPath, Path.GetFileName(fileName));

        if (!System.IO.File.Exists(filePath))
            return NotFound("No download available yet.");

        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, "application/octet-stream", fileName);
    }
}
