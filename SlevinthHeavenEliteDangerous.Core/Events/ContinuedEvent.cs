using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ContinuedEvent : EventBase
{
    [JsonPropertyName("Part")]
    public int Part { get; set; } = 0;
}
