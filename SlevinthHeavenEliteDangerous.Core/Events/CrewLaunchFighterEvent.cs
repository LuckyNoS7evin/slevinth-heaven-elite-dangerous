using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CrewLaunchFighterEvent : EventBase
{
    [JsonPropertyName("Crew")]
    public string Crew { get; set; } = string.Empty;

    [JsonPropertyName("ID")]
    public int? ID { get; set; }

    [JsonPropertyName("Telepresence")]
    public bool? Telepresence { get; set; }
}
