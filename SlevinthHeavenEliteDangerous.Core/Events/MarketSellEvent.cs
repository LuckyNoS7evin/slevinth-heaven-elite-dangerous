using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class MarketSellEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Type_Localised")]
    public string Type_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int? Count { get; set; }

    [JsonPropertyName("SellPrice")]
    public long? SellPrice { get; set; }

    [JsonPropertyName("TotalSale")]
    public long? TotalSale { get; set; }

    [JsonPropertyName("AvgPricePaid")]
    public long? AvgPricePaid { get; set; }
}
