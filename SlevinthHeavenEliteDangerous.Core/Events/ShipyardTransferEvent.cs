using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ShipyardTransferEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("ShipType")]
    public string ShipType { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int? ShipID { get; set; }

    [JsonPropertyName("System")]
    public string System { get; set; } = string.Empty;

    [JsonPropertyName("ShipMarketID")]
    public long? ShipMarketID { get; set; }

    [JsonPropertyName("Distance")]
    public double? Distance { get; set; }

    [JsonPropertyName("TransferPrice")]
    public long? TransferPrice { get; set; }

    [JsonPropertyName("TransferTime")]
    public long? TransferTime { get; set; }
}
