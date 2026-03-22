using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Persisted state for <see cref="JournalUploadService"/> tracking which
/// journal files have been uploaded and when.
/// </summary>
public class JournalUploadState
{
    /// <summary>
    /// The Frontier ID of the commander whose journals are being uploaded.
    /// Discovered by scanning journal files for Commander/LoadGame events.
    /// </summary>
    public string? FID { get; set; }

    /// <summary>
    /// Per-file record of the last-uploaded size (in bytes).
    /// If the file grows (new events appended), it will be re-uploaded.
    /// Key = file name (e.g. "Journal.2024-01-15T120000.01.log").
    /// </summary>
    public Dictionary<string, long> UploadedFiles { get; set; } = new();
}
