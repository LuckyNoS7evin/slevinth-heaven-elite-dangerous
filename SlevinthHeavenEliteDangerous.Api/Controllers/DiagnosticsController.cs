using Microsoft.AspNetCore.Mvc;
using SlevinthHeavenEliteDangerous.Api.Discord;
using SlevinthHeavenEliteDangerous.Core.Models;

namespace SlevinthHeavenEliteDangerous.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class DiagnosticsController(
    IDiscordDiagnosticsService discordDiagnostics,
    ILogger<DiagnosticsController> logger) : ControllerBase
{
    [HttpPost]
    public IActionResult Post([FromBody] DiagnosticsReport report)
    {
        var commanderName = HttpContext.Items["CommanderName"] as string ?? "Unknown";

        logger.LogInformation(
            "[Diagnostics] Report from CMDR {Commander} (generated {GeneratedAt}): " +
            "{MissingEventCount} missing events, {MissingPropertyCount} events with missing properties, " +
            "{RankPropertyCount} events with rank properties, {SerializationFailureCount} serialization failures",
            commanderName,
            report.GeneratedAt,
            report.MissingEvents.Count,
            report.MissingProperties.Count,
            report.EventsWithRankProperties.Count,
            report.SerializationFailures.Count);

        // Fire-and-forget so the HTTP response is returned immediately while Discord messages
        // are sent in the background (rate-limited at ~1 message per second).
        _ = discordDiagnostics.SendReportAsync(report, commanderName);

        return Ok();
    }
}
