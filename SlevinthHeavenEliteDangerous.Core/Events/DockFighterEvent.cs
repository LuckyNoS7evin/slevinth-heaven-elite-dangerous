using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class DockFighterEvent : EventBase
{
    [JsonPropertyName("ID")]
    public int? ID { get; set; }
}
