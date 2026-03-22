using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class DropItemsEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("OwnerID")]
    public long? OwnerID { get; set; }

    [JsonPropertyName("MissionID")]
    public long? MissionID { get; set; }

    [JsonPropertyName("Count")]
    public int? Count { get; set; }
}
