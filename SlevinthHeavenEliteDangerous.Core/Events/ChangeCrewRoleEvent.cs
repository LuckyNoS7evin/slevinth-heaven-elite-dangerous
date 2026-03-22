using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ChangeCrewRoleEvent : EventBase
{
    [JsonPropertyName("Role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("Telepresence")]
    public bool? Telepresence { get; set; }
}
