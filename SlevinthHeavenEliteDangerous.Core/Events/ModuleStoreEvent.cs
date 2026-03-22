using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ModuleStoreEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("Slot")]
    public string Slot { get; set; } = string.Empty;

    [JsonPropertyName("StoredItem")]
    public string StoredItem { get; set; } = string.Empty;

    [JsonPropertyName("StoredItem_Localised")]
    public string StoredItem_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int ShipID { get; set; }

    [JsonPropertyName("Hot")]
    public bool Hot { get; set; }
}
