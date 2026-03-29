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
    public string VictimFaction { get; }
    public bool IsPvp { get; }
    public int? VictimCombatRank { get; }
    public long CreditsEarned { get; }
    public string KillType { get; }

    public string TimestampFormatted => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

    public string KillTypeBadge => KillType switch
    {
        "Bounty"     => "NPC",
        "CombatBond" => "CZ",
        "CapShipBond"=> "CAP",
        "PVP"        => "PVP",
        _            => KillType,
    };

    public string VictimRankName => VictimCombatRank.HasValue
        ? CombatRankHelper.GetRankName(VictimCombatRank.Value)
        : string.Empty;

    public string CreditsEarnedFormatted => CreditsEarned > 0
        ? CreditsEarned.ToString("N0") + " CR"
        : string.Empty;

    public CombatKillEntryViewModel(CombatKillRecord record)
    {
        Timestamp        = record.Timestamp;
        Target           = record.Target;
        VictimFaction    = record.VictimFaction;
        IsPvp            = record.IsPvp;
        VictimCombatRank = record.VictimCombatRank;
        CreditsEarned    = record.CreditsEarned;
        KillType         = record.KillType;
    }

    public static CombatKillEntryViewModel FromRecord(CombatKillRecord record) => new(record);
}
