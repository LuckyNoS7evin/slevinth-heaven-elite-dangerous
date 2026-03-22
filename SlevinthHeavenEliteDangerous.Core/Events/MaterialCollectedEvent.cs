using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class MaterialCollectedEvent : EventBase
{
    [JsonPropertyName("Category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int? Count { get; set; }
}
