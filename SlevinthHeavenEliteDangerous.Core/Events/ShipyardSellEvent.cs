using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ShipyardSellEvent : EventBase
{
    [JsonPropertyName("ShipType")]
    public string ShipType { get; set; } = string.Empty;

    [JsonPropertyName("ShipType_Localised")]
    public string ShipType_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SellShipID")]
    public int? SellShipID { get; set; }

    [JsonPropertyName("ShipPrice")]
    public long? ShipPrice { get; set; }

    [JsonPropertyName("System")]
    public string System { get; set; } = string.Empty;

    [JsonPropertyName("ShipMarketID")]
    public long? ShipMarketID { get; set; }

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }
}
