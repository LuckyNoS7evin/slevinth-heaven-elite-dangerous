using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CrewFireEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("CrewID")]
    public long CrewID { get; set; }
}
