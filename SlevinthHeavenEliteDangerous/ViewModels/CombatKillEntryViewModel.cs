using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.Services.Models;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// View model for a single row in the combat kill log.
/// </summary>
public class CombatKillEntryViewModel
{
    public DateTime Timestamp { get; }
    public string Target { get; }
    public string? PilotName { get; }
    public string VictimFaction { get; }
    public bool IsPvp { get; }
    public int? VictimCombatRank { get; }
    public string? VictimPilotRank { get; }
    public long CreditsEarned { get; }
    public string KillType { get; }
    public int? PlayerCombatRank { get; }

    public string TimestampFormatted => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

    public string KillTypeBadge => KillType switch
    {
        "Bounty"     => "NPC",
        "CombatBond" => "CZ",
        "CapShipBond"=> "CAP",
        "PVP"        => "PVP",
        _            => KillType,
    };

    public string VictimRankName
    {
        get
        {
            // For PVP kills, use VictimCombatRank (int)
            if (VictimCombatRank.HasValue)
                return CombatRankHelper.GetRankName(VictimCombatRank.Value);

            // For NPC kills, use VictimPilotRank (string) if available
            if (!string.IsNullOrEmpty(VictimPilotRank))
                return VictimPilotRank;

            return string.Empty;
        }
    }

    public int? VictimRankInt
    {
        get
        {
            // For PVP, return the int rank directly
            if (VictimCombatRank.HasValue)
                return VictimCombatRank.Value;

            // For NPC, convert string rank to int
            return CombatRankHelper.GetRankFromName(VictimPilotRank);
        }
    }

    public string RankComparisonDisplay
    {
        get
        {
            if (!VictimRankInt.HasValue || !PlayerCombatRank.HasValue)
                return string.Empty;

            int diff = VictimRankInt.Value - PlayerCombatRank.Value;

            if (diff == 0)
                return "(Equal Rank)";
            else if (diff > 0)
                return $"(+{diff} {(diff == 1 ? "rank" : "ranks")} above you)";
            else
                return $"({-diff} {(-diff == 1 ? "rank" : "ranks")} below you)";
        }
    }

    public string CreditsEarnedFormatted => CreditsEarned > 0
        ? CreditsEarned.ToString("N0") + " CR"
        : string.Empty;

    public CombatKillEntryViewModel(CombatKillRecord record, int? playerCombatRank = null)
    {
        Timestamp        = record.Timestamp;
        Target           = record.Target;
        PilotName        = record.PilotName;
        VictimFaction    = record.VictimFaction;
        IsPvp            = record.IsPvp;
        VictimCombatRank = record.VictimCombatRank;
        VictimPilotRank  = record.VictimPilotRank;
        CreditsEarned    = record.CreditsEarned;
        KillType         = record.KillType;
        PlayerCombatRank = playerCombatRank;
    }

    public static CombatKillEntryViewModel FromRecord(CombatKillRecord record, int? playerCombatRank = null) 
        => new(record, playerCombatRank);
}
