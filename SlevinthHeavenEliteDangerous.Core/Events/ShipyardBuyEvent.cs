using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ShipyardBuyEvent : EventBase
{
    [JsonPropertyName("ShipType")]
    public string ShipType { get; set; } = string.Empty;

    [JsonPropertyName("ShipType_Localised")]
    public string ShipType_Localised { get; set; } = string.Empty;

    [JsonPropertyName("ShipPrice")]
    public long? ShipPrice { get; set; }

    [JsonPropertyName("StoreOldShip")]
    public string StoreOldShip { get; set; } = string.Empty;

    [JsonPropertyName("StoreShipID")]
    public int? StoreShipID { get; set; }

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }
}
