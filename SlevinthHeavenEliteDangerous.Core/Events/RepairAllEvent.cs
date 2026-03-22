using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class RepairAllEvent : EventBase
{
    [JsonPropertyName("Cost")]
    public long Cost { get; set; }
}
