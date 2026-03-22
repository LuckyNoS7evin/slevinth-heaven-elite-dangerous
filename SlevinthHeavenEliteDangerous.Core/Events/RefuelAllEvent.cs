using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class RefuelAllEvent : EventBase
{
    [JsonPropertyName("Cost")]
    public long Cost { get; set; }

    [JsonPropertyName("Amount")]
    public double Amount { get; set; }
}
