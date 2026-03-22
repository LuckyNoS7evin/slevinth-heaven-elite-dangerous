using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class SmugglingStats
{
    [JsonPropertyName("Black_Markets_Traded_With")]
    public int? BlackMarketsTradedWith { get; set; }

    [JsonPropertyName("Black_Markets_Profits")]
    public long? BlackMarketsProfits { get; set; }

    [JsonPropertyName("Resources_Smuggled")]
    public int? ResourcesSmuggled { get; set; }

    [JsonPropertyName("Average_Profit")]
    public double? AverageProfit { get; set; }

    [JsonPropertyName("Highest_Single_Transaction")]
    public long? HighestSingleTransaction { get; set; }
}
