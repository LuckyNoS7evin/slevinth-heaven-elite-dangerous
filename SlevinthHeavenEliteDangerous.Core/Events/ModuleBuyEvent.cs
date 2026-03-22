using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ModuleBuyEvent : EventBase
{
    [JsonPropertyName("Slot")]
    public string Slot { get; set; } = string.Empty;

    [JsonPropertyName("BuyItem")]
    public string BuyItem { get; set; } = string.Empty;

    [JsonPropertyName("BuyItem_Localised")]
    public string BuyItem_Localised { get; set; } = string.Empty;

    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("BuyPrice")]
    public long BuyPrice { get; set; }

    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int ShipID { get; set; }

    [JsonPropertyName("StoredItem")]
    public string StoredItem { get; set; } = string.Empty;

    [JsonPropertyName("StoredItem_Localised")]
    public string StoredItem_Localised { get; set; } = string.Empty;
}
