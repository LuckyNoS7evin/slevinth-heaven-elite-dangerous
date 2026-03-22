namespace SlevinthHeavenEliteDangerous.Services.Models;

public class OverlayLogEntryRecord
{
    public string EntryType { get; set; } = string.Empty;
    public string TimeText { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string SubText { get; set; } = string.Empty;
    public string ValueText { get; set; } = string.Empty;
    public string? Key { get; set; }
    public DateTime? Time { get; set; }
}
