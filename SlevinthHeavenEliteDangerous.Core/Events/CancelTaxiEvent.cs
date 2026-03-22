using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CancelTaxiEvent : EventBase
{
    [JsonPropertyName("Refund")]
    public long? Refund { get; set; }
}
