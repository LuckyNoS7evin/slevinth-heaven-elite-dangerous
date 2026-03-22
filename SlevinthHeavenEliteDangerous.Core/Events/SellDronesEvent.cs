using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SellDronesEvent : EventBase
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int Count { get; set; } = 0;

    [JsonPropertyName("SellPrice")]
    public long SellPrice { get; set; } = 0;

    [JsonPropertyName("TotalSale")]
    public long TotalSale { get; set; } = 0;
}
