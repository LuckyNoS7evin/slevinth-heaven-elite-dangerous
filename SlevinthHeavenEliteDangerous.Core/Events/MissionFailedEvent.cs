using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class MissionFailedEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("MissionID")]
    public long MissionID { get; set; } = 0;

    [JsonPropertyName("Fine")]
    public long? Fine { get; set; }
}
