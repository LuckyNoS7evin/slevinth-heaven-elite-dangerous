namespace SlevinthHeavenEliteDangerous.Eddn;

/// <summary>
/// Per-commander EDDN state persisted to Data/Journals/{FID}/_eddn_state.json.
/// </summary>
public sealed class EddnCommanderState
{
    public EddnGameInfo GameInfo { get; set; } = new();

    /// <summary>Current system name — updated on FSDJump/Location, used to augment scan messages.</summary>
    public string? CurrentSystem { get; set; }

    /// <summary>Current system address — used to cross-check scan events.</summary>
    public long? CurrentSystemAddress { get; set; }

    /// <summary>Current star position — used to augment scan messages.</summary>
    public double[]? CurrentStarPos { get; set; }

    /// <summary>Scan/SAASignalsFound/FSSBodySignals events held until exploration data is sold.</summary>
    public List<EddnPendingEvent> PendingNavEvents { get; set; } = [];

    /// <summary>CodexEntry events held until organic data is sold.</summary>
    public List<EddnPendingEvent> PendingCodexEvents { get; set; } = [];

    /// <summary>Dedup keys for events already sent — prevents re-sending on full reprocess.</summary>
    public HashSet<string> SentEventKeys { get; set; } = [];

    /// <summary>
    /// Mapping of system address to last-known star position (if available).
    /// Used to augment pending messages that lack StarPos before sending.
    /// </summary>
    public Dictionary<long, double[]?> SystemPositions { get; set; } = new();
}

public sealed class EddnGameInfo
{
    public string GameVersion { get; set; } = string.Empty;
    public string GameBuild { get; set; } = string.Empty;
    public bool? Horizons { get; set; }
    public bool? Odyssey { get; set; }

    /// <summary>Used as the EDDN uploaderID.</summary>
    public string CommanderName { get; set; } = string.Empty;
}

public sealed class EddnPendingEvent
{
    /// <summary>Dedup key — checked against SentEventKeys before sending.</summary>
    public string EventKey { get; set; } = string.Empty;

    /// <summary>EDDN schema ref, e.g. "https://eddn.edcd.io/schemas/journal/1".</summary>
    public string SchemaRef { get; set; } = string.Empty;

    /// <summary>Already-sanitised and augmented message JSON.</summary>
    public string MessageJson { get; set; } = string.Empty;

    /// <summary>SystemAddress for grouping; 0 if not applicable (e.g. CodexEntry).</summary>
    public long SystemAddress { get; set; }
}
