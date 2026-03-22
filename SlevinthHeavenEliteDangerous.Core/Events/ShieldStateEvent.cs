using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ShieldStateEvent : EventBase
{
    [JsonPropertyName("ShieldsUp")]
    public bool? ShieldsUp { get; set; }
}
