using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ShipyardSwapEvent : EventBase
{
    [JsonPropertyName("ShipType")]
    public string ShipType { get; set; } = string.Empty;

    [JsonPropertyName("ShipType_Localised")]
    public string ShipType_Localised { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int? ShipID { get; set; }

    [JsonPropertyName("StoreOldShip")]
    public string StoreOldShip { get; set; } = string.Empty;

    [JsonPropertyName("StoreShipID")]
    public int? StoreShipID { get; set; }

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }
}
