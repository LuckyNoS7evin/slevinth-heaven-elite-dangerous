using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class MarketBuyEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Type_Localised")]
    public string Type_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("BuyPrice")]
    public int BuyPrice { get; set; }

    [JsonPropertyName("TotalCost")]
    public int TotalCost { get; set; }
}
