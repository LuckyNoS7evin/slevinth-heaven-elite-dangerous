using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CombatStats
{
    [JsonPropertyName("Bounties_Claimed")]
    public int? BountiesClaimed { get; set; }

    [JsonPropertyName("Bounty_Hunting_Profit")]
    public long? BountyHuntingProfit { get; set; }

    [JsonPropertyName("Combat_Bonds")]
    public int? CombatBonds { get; set; }

    [JsonPropertyName("Combat_Bond_Profits")]
    public long? CombatBondProfits { get; set; }

    [JsonPropertyName("Assassinations")]
    public int? Assassinations { get; set; }

    [JsonPropertyName("Assassination_Profits")]
    public long? AssassinationProfits { get; set; }

    [JsonPropertyName("Highest_Single_Reward")]
    public long? HighestSingleReward { get; set; }

    [JsonPropertyName("Skimmers_Killed")]
    public int? SkimmersKilled { get; set; }

    [JsonPropertyName("OnFoot_Combat_Bonds")]
    public int? OnFootCombatBonds { get; set; }

    [JsonPropertyName("OnFoot_Combat_Bonds_Profits")]
    public long? OnFootCombatBondsProfits { get; set; }

    [JsonPropertyName("OnFoot_Vehicles_Destroyed")]
    public int? OnFootVehiclesDestroyed { get; set; }

    [JsonPropertyName("OnFoot_Ships_Destroyed")]
    public int? OnFootShipsDestroyed { get; set; }

    [JsonPropertyName("Dropships_Taken")]
    public int? DropshipsTaken { get; set; }

    [JsonPropertyName("Dropships_Booked")]
    public int? DropshipsBooked { get; set; }

    [JsonPropertyName("Dropships_Cancelled")]
    public int? DropshipsCancelled { get; set; }

    [JsonPropertyName("ConflictZone_High")]
    public int? ConflictZoneHigh { get; set; }

    [JsonPropertyName("ConflictZone_Medium")]
    public int? ConflictZoneMedium { get; set; }

    [JsonPropertyName("ConflictZone_Low")]
    public int? ConflictZoneLow { get; set; }

    [JsonPropertyName("ConflictZone_Total")]
    public int? ConflictZoneTotal { get; set; }

    [JsonPropertyName("ConflictZone_High_Wins")]
    public int? ConflictZoneHighWins { get; set; }

    [JsonPropertyName("ConflictZone_Medium_Wins")]
    public int? ConflictZoneMediumWins { get; set; }

    [JsonPropertyName("ConflictZone_Low_Wins")]
    public int? ConflictZoneLowWins { get; set; }

    [JsonPropertyName("ConflictZone_Total_Wins")]
    public int? ConflictZoneTotalWins { get; set; }

    [JsonPropertyName("Settlement_Defended")]
    public int? SettlementDefended { get; set; }

    [JsonPropertyName("Settlement_Conquered")]
    public int? SettlementConquered { get; set; }

    [JsonPropertyName("OnFoot_Skimmers_Killed")]
    public int? OnFootSkimmersKilled { get; set; }

    [JsonPropertyName("OnFoot_Scavs_Killed")]
    public int? OnFootScavsKilled { get; set; }
}
