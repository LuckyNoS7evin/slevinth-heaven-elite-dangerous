using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CancelDropshipEvent : EventBase
{
    [JsonPropertyName("Refund")]
    public long? Refund { get; set; }
}
