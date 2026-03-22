using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Persisted state for all logged codex first-discoveries.
/// </summary>
public class CodexStateModel
{
    public List<CodexEntryModel> Entries { get; set; } = [];
}
