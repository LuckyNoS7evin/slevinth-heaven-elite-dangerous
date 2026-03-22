using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CrewAssignEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("CrewID")]
    public long CrewID { get; set; } = 0;

    [JsonPropertyName("Role")]
    public string Role { get; set; } = string.Empty;
}
