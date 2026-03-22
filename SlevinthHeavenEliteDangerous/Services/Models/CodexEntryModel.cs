using System;

namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// A single first-discovery codex entry logged by the commander.
/// </summary>
public class CodexEntryModel
{
    public long EntryID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string System { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public long? VoucherAmount { get; set; }
}
