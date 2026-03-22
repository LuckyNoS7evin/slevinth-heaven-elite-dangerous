using SlevinthHeavenEliteDangerous.Core.Models;

namespace SlevinthHeavenEliteDangerous.Api.Discord;

public interface IDiscordDiagnosticsService
{
    Task SendReportAsync(DiagnosticsReport report, string commanderName);
}
