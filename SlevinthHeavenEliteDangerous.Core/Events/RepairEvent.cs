using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class RepairEvent : EventBase
{
    [JsonPropertyName("Item")]
    public string Item { get; set; } = string.Empty;

    [JsonPropertyName("Items")]
    public List<string> Items { get; set; } = [];

    [JsonPropertyName("Cost")]
    public long? Cost { get; set; }
}
