using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class QuitACrewEvent : EventBase
{
    [JsonPropertyName("Captain")]
    public string Captain { get; set; } = string.Empty;

    [JsonPropertyName("Telepresence")]
    public bool? Telepresence { get; set; }
}
