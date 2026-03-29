namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Persisted state for the combat log
/// </summary>
public class CombatStateModel
{
    public int NpcKillCount { get; set; }
    public int PvpKillCount { get; set; }
    public long TotalBountyCredits { get; set; }
    public long TotalCombatBondCredits { get; set; }
    public long TotalCapShipBondCredits { get; set; }
    public int DeathCount { get; set; }
    public int InterdictionsAttempted { get; set; }
    public int InterdictionsSucceeded { get; set; }
    public int TimesInterdicted { get; set; }
    public int InterdictionsEscaped { get; set; }
    public double HullLowWaterMark { get; set; } = 1.0;
    public int CrimeCount { get; set; }
    public Dictionary<string, int> KillsByFaction { get; set; } = [];
    public List<CombatKillRecord> KillLog { get; set; } = [];
}

/// <summary>
/// A single entry in the combat kill log
/// </summary>
public class CombatKillRecord
{
    public DateTime Timestamp { get; set; }

    /// <summary>Ship type for NPC kills; victim pilot name for PVP kills</summary>
    public string Target { get; set; } = string.Empty;

    public string VictimFaction { get; set; } = string.Empty;
    public bool IsPvp { get; set; }

    /// <summary>Victim combat rank integer — populated for PVP kills only</summary>
    public int? VictimCombatRank { get; set; }

    public long CreditsEarned { get; set; }

    /// <summary>"Bounty", "CombatBond", "CapShipBond", or "PVP"</summary>
    public string KillType { get; set; } = string.Empty;
}
