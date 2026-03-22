using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class EndCrewSessionEvent : EventBase
{
    [JsonPropertyName("OnCrime")]
    public bool? OnCrime { get; set; }

    [JsonPropertyName("Telepresence")]
    public bool? Telepresence { get; set; }
}
