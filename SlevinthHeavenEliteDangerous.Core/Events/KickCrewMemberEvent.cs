using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class KickCrewMemberEvent : EventBase
{
    [JsonPropertyName("Crew")]
    public string Crew { get; set; } = string.Empty;

    [JsonPropertyName("OnCrime")]
    public bool? OnCrime { get; set; }

    [JsonPropertyName("Telepresence")]
    public bool? Telepresence { get; set; }
}
