using System;

namespace SlevinthHeavenEliteDangerous.ViewModels;

public enum OverlayLogEntryType { BodyScan, ExoBio }

/// <summary>
/// A single line in the overlay scan log.
/// </summary>
public class OverlayLogEntry
{
    public OverlayLogEntryType EntryType { get; init; }
    public string TimeText { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string SubText { get; init; } = string.Empty;
    public string ValueText { get; init; } = string.Empty;

    /// <summary>Unique key used to find and update ExoBio entries as they progress.</summary>
    public string? Key { get; init; }
}
