using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ShipTargetedEvent : EventBase
{
    [JsonPropertyName("TargetLocked")]
    public bool TargetLocked { get; set; }

    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("Ship_Localised")]
    public string Ship_Localised { get; set; } = string.Empty;

    [JsonPropertyName("ScanStage")]
    public int? ScanStage { get; set; }

    [JsonPropertyName("PilotName")]
    public string PilotName { get; set; } = string.Empty;

    [JsonPropertyName("PilotName_Localised")]
    public string PilotName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("PilotRank")]
    public string PilotRank { get; set; } = string.Empty;

    [JsonPropertyName("ShieldHealth")]
    public double? ShieldHealth { get; set; }

    [JsonPropertyName("HullHealth")]
    public double? HullHealth { get; set; }

    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("LegalStatus")]
    public string LegalStatus { get; set; } = string.Empty;

    [JsonPropertyName("Bounty")]
    public int? Bounty { get; set; }

    [JsonPropertyName("SubSystem")]
    public string SubSystem { get; set; } = string.Empty;

    [JsonPropertyName("SubSystemHealth")]
    public double? SubSystemHealth { get; set; }
}
