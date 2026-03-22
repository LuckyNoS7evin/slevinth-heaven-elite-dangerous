using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class AsteroidCrackedEvent : EventBase
{
    [JsonPropertyName("Body")]
    public string Body { get; set; } = string.Empty;
}
