using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class FighterDestroyedEvent : EventBase
{
    [JsonPropertyName("ID")]
    public int? ID { get; set; }
}
