using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class DatalinkVoucherEvent : EventBase
{
    [JsonPropertyName("Reward")]
    public long? Reward { get; set; }

    [JsonPropertyName("VictimFaction")]
    public string VictimFaction { get; set; } = string.Empty;

    [JsonPropertyName("PayeeFaction")]
    public string PayeeFaction { get; set; } = string.Empty;
}
