using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class MaterialDiscoveredEvent : EventBase
{
    [JsonPropertyName("Category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("DiscoveryNumber")]
    public int? DiscoveryNumber { get; set; }
}
