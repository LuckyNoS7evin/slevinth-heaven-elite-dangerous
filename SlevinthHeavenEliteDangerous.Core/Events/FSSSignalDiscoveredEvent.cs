using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class FSSSignalDiscoveredEvent : EventBase
{
    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("SignalName")]
    public string SignalName { get; set; } = string.Empty;

    [JsonPropertyName("SignalName_Localised")]
    public string SignalName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SignalType")]
    public string SignalType { get; set; } = string.Empty;

    [JsonPropertyName("IsStation")]
    public bool? IsStation { get; set; }

    [JsonPropertyName("SpawningFaction")]
    public string SpawningFaction { get; set; } = string.Empty;

    [JsonPropertyName("SpawningFaction_Localised")]
    public string SpawningFaction_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SpawningState")]
    public string SpawningState { get; set; } = string.Empty;

    [JsonPropertyName("SpawningState_Localised")]
    public string SpawningState_Localised { get; set; } = string.Empty;

    [JsonPropertyName("ThreatLevel")]
    public int? ThreatLevel { get; set; }

    [JsonPropertyName("TimeRemaining")]
    public double? TimeRemaining { get; set; }

    [JsonPropertyName("USSType")]
    public string USSType { get; set; } = string.Empty;

    [JsonPropertyName("USSType_Localised")]
    public string USSType_Localised { get; set; } = string.Empty;
}
