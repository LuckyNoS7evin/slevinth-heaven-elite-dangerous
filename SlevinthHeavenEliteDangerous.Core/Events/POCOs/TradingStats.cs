using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class TradingStats
{
    [JsonPropertyName("Markets_Traded_With")]
    public int? MarketsTradedWith { get; set; }

    [JsonPropertyName("Market_Profits")]
    public long? MarketProfits { get; set; }

    [JsonPropertyName("Resources_Traded")]
    public int? ResourcesTraded { get; set; }

    [JsonPropertyName("Average_Profit")]
    public double? AverageProfit { get; set; }

    [JsonPropertyName("Highest_Single_Transaction")]
    public long? HighestSingleTransaction { get; set; }

    [JsonPropertyName("Data_Sold")]
    public int? DataSold { get; set; }

    [JsonPropertyName("Goods_Sold")]
    public int? GoodsSold { get; set; }

    [JsonPropertyName("Assets_Sold")]
    public int? AssetsSold { get; set; }
}
