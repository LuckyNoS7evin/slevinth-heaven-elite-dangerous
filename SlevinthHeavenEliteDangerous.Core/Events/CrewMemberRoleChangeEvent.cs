using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CrewMemberRoleChangeEvent : EventBase
{
    [JsonPropertyName("Crew")]
    public string Crew { get; set; } = string.Empty;

    [JsonPropertyName("Role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("Telepresence")]
    public bool? Telepresence { get; set; }
}
