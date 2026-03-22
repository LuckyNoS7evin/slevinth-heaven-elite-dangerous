using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ModuleSellEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("Slot")]
    public string Slot { get; set; } = string.Empty;

    [JsonPropertyName("SellItem")]
    public string SellItem { get; set; } = string.Empty;

    [JsonPropertyName("SellItem_Localised")]
    public string SellItem_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SellPrice")]
    public long SellPrice { get; set; } = 0;

    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int? ShipID { get; set; }
}
