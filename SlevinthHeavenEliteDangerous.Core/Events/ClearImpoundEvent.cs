using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ClearImpoundEvent : EventBase
{
    [JsonPropertyName("ShipType")]
    public string ShipType { get; set; } = string.Empty;

    [JsonPropertyName("ShipType_Localised")]
    public string ShipType_Localised { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int? ShipID { get; set; }

    [JsonPropertyName("ShipMarketID")]
    public long? ShipMarketID { get; set; }

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }
}
