using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class BuyAmmoEvent : EventBase
{
    [JsonPropertyName("Cost")]
    public long Cost { get; set; }
}
