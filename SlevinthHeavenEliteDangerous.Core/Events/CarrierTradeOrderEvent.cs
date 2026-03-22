using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierTradeOrderEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("BlackMarket")]
    public bool? BlackMarket { get; set; }

    [JsonPropertyName("Commodity")]
    public string Commodity { get; set; } = string.Empty;

    [JsonPropertyName("Commodity_Localised")]
    public string Commodity_Localised { get; set; } = string.Empty;

    [JsonPropertyName("PurchaseOrder")]
    public int? PurchaseOrder { get; set; }

    [JsonPropertyName("SaleOrder")]
    public int? SaleOrder { get; set; }

    [JsonPropertyName("CancelTrade")]
    public bool? CancelTrade { get; set; }

    [JsonPropertyName("Price")]
    public int? Price { get; set; }
}
