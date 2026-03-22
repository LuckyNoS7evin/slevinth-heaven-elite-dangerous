using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class MissionCompletedEvent : EventBase
{
    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("LocalisedName")]
    public string LocalisedName { get; set; } = string.Empty;

    [JsonPropertyName("MissionID")]
    public long MissionID { get; set; }

    [JsonPropertyName("Reward")]
    public long? Reward { get; set; }

    [JsonPropertyName("Donation")]
    public string Donation { get; set; } = string.Empty;

    [JsonPropertyName("Donated")]
    public long? Donated { get; set; }

    [JsonPropertyName("TargetFaction")]
    public string TargetFaction { get; set; } = string.Empty;

    [JsonPropertyName("Commodity")]
    public string Commodity { get; set; } = string.Empty;

    [JsonPropertyName("Commodity_Localised")]
    public string Commodity_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int? Count { get; set; }

    [JsonPropertyName("DestinationSystem")]
    public string DestinationSystem { get; set; } = string.Empty;

    [JsonPropertyName("DestinationStation")]
    public string DestinationStation { get; set; } = string.Empty;

    [JsonPropertyName("FactionEffects")]
    public List<FactionEffect> FactionEffects { get; set; } = [];
}
