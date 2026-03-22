using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class MissionAcceptedEvent : EventBase
{
    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("LocalisedName")]
    public string LocalisedName { get; set; } = string.Empty;

    [JsonPropertyName("Commodity")]
    public string Commodity { get; set; } = string.Empty;

    [JsonPropertyName("Commodity_Localised")]
    public string Commodity_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int? Count { get; set; }

    [JsonPropertyName("TargetFaction")]
    public string TargetFaction { get; set; } = string.Empty;

    [JsonPropertyName("DestinationSystem")]
    public string DestinationSystem { get; set; } = string.Empty;

    [JsonPropertyName("DestinationStation")]
    public string DestinationStation { get; set; } = string.Empty;

    [JsonPropertyName("Expiry")]
    public DateTime? Expiry { get; set; }

    [JsonPropertyName("Wing")]
    public bool Wing { get; set; }

    [JsonPropertyName("Donation")]
    public string Donation { get; set; } = string.Empty;

    [JsonPropertyName("KillCount")]
    public int? KillCount { get; set; }

    [JsonPropertyName("TargetType")]
    public string TargetType { get; set; } = string.Empty;

    [JsonPropertyName("TargetType_Localised")]
    public string TargetType_Localised { get; set; } = string.Empty;

    [JsonPropertyName("PassengerCount")]
    public int? PassengerCount { get; set; }

    [JsonPropertyName("PassengerType")]
    public string PassengerType { get; set; } = string.Empty;

    [JsonPropertyName("PassengerVIPs")]
    public bool? PassengerVIPs { get; set; }

    [JsonPropertyName("PassengerWanted")]
    public bool? PassengerWanted { get; set; }

    [JsonPropertyName("Influence")]
    public string Influence { get; set; } = string.Empty;

    [JsonPropertyName("Reputation")]
    public string Reputation { get; set; } = string.Empty;

    [JsonPropertyName("Reward")]
    public long? Reward { get; set; }

    [JsonPropertyName("MissionID")]
    public long MissionID { get; set; }
}
