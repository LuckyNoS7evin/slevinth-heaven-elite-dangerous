using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class LaunchSRVEvent : EventBase
{
    [JsonPropertyName("SRVType")]
    public string SRVType { get; set; } = string.Empty;

    [JsonPropertyName("SRVType_Localised")]
    public string SRVType_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Loadout")]
    public string Loadout { get; set; } = string.Empty;

    [JsonPropertyName("ID")]
    public int ID { get; set; }

    [JsonPropertyName("PlayerControlled")]
    public bool PlayerControlled { get; set; }
}
