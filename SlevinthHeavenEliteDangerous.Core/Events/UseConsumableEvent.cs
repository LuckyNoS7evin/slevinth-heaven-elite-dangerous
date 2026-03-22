using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class UseConsumableEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;
}
