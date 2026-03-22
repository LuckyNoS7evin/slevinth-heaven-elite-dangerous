using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class PowerplayDeliverEvent : EventBase
{
    [JsonPropertyName("Power")]
    public string Power { get; set; } = string.Empty;

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int Count { get; set; } = 0;
}
