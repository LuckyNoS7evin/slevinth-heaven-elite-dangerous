using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierDecommissionEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("ScrapRefund")]
    public long ScrapRefund { get; set; } = 0;

    [JsonPropertyName("ScrapTime")]
    public long? ScrapTime { get; set; }
}
