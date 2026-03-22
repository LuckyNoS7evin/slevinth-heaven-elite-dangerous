using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class BountyEvent : EventBase
{
    [JsonPropertyName("Rewards")]
    public List<BountyReward> Rewards { get; set; } = [];

    [JsonPropertyName("PilotName")]
    public string PilotName { get; set; } = string.Empty;

    [JsonPropertyName("PilotName_Localised")]
    public string PilotName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Target")]
    public string Target { get; set; } = string.Empty;

    [JsonPropertyName("Target_Localised")]
    public string Target_Localised { get; set; } = string.Empty;

    [JsonPropertyName("TotalReward")]
    public long? TotalReward { get; set; }

    [JsonPropertyName("VictimFaction")]
    public string VictimFaction { get; set; } = string.Empty;
}
