using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class RefuelPartialEvent : EventBase
{
    [JsonPropertyName("Cost")]
    public long Cost { get; set; } = 0;

    [JsonPropertyName("Amount")]
    public double Amount { get; set; } = 0;
}
