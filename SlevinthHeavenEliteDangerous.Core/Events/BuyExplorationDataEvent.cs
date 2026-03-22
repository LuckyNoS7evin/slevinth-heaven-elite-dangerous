using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class BuyExplorationDataEvent : EventBase
{
    [JsonPropertyName("System")]
    public string System { get; set; } = string.Empty;

    [JsonPropertyName("Cost")]
    public long? Cost { get; set; }
}
