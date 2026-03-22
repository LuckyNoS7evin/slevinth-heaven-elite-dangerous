using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ModuleSwapEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("FromSlot")]
    public string FromSlot { get; set; } = string.Empty;

    [JsonPropertyName("ToSlot")]
    public string ToSlot { get; set; } = string.Empty;

    [JsonPropertyName("FromItem")]
    public string FromItem { get; set; } = string.Empty;

    [JsonPropertyName("ToItem")]
    public string ToItem { get; set; } = string.Empty;

    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int? ShipID { get; set; }
}
