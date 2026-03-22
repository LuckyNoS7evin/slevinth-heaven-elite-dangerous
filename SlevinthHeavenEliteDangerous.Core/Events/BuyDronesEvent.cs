using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class BuyDronesEvent : EventBase
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("BuyPrice")]
    public long BuyPrice { get; set; }

    [JsonPropertyName("TotalCost")]
    public long TotalCost { get; set; }
}
