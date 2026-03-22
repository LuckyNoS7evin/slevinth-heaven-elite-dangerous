using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class MissionAbandonedEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("LocalisedName")]
    public string LocalisedName { get; set; } = string.Empty;

    [JsonPropertyName("MissionID")]
    public long? MissionID { get; set; }

    [JsonPropertyName("Fine")]
    public long? Fine { get; set; }
}
