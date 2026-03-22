using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CrewMemberJoinsEvent : EventBase
{
    [JsonPropertyName("Crew")]
    public string Crew { get; set; } = string.Empty;

    [JsonPropertyName("Telepresence")]
    public bool? Telepresence { get; set; }
}
