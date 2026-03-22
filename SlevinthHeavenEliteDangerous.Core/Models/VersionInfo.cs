namespace SlevinthHeavenEliteDangerous.Core.Models;

public class VersionInfo
{
    public string LatestVersion { get; set; } = string.Empty;
    public string? ReleaseNotesUrl { get; set; }
    public string? DownloadUrl { get; set; }
}
