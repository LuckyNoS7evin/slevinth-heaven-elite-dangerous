using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CrimeStats
{
    [JsonPropertyName("Notoriety")]
    public int? Notoriety { get; set; }

    [JsonPropertyName("Fines")]
    public long? Fines { get; set; }

    [JsonPropertyName("Total_Fines")]
    public long? TotalFines { get; set; }

    [JsonPropertyName("Bounties_Received")]
    public int? BountiesReceived { get; set; }

    [JsonPropertyName("Total_Bounties")]
    public long? TotalBounties { get; set; }

    [JsonPropertyName("Highest_Bounty")]
    public long? HighestBounty { get; set; }

    [JsonPropertyName("Malware_Uploaded")]
    public int? MalwareUploaded { get; set; }

    [JsonPropertyName("Settlements_State_Shutdown")]
    public int? SettlementsStateShutdown { get; set; }

    [JsonPropertyName("Production_Sabotage")]
    public int? ProductionSabotage { get; set; }

    [JsonPropertyName("Production_Theft")]
    public int? ProductionTheft { get; set; }

    [JsonPropertyName("Total_Murders")]
    public int? TotalMurders { get; set; }

    [JsonPropertyName("Citizens_Murdered")]
    public int? CitizensMurdered { get; set; }

    [JsonPropertyName("Omnipol_Murdered")]
    public int? OmnipolMurdered { get; set; }

    [JsonPropertyName("Guards_Murdered")]
    public int? GuardsMurdered { get; set; }

    [JsonPropertyName("Data_Stolen")]
    public int? DataStolen { get; set; }

    [JsonPropertyName("Goods_Stolen")]
    public int? GoodsStolen { get; set; }

    [JsonPropertyName("Sample_Stolen")]
    public int? SampleStolen { get; set; }

    [JsonPropertyName("Total_Stolen")]
    public int? TotalStolen { get; set; }

    [JsonPropertyName("Turrets_Destroyed")]
    public int? TurretsDestroyed { get; set; }

    [JsonPropertyName("Turrets_Overloaded")]
    public int? TurretsOverloaded { get; set; }

    [JsonPropertyName("Turrets_Total")]
    public int? TurretsTotal { get; set; }

    [JsonPropertyName("Value_Stolen_StateChange")]
    public long? ValueStolenStateChange { get; set; }

    [JsonPropertyName("Profiles_Cloned")]
    public int? ProfilesCloned { get; set; }
}
