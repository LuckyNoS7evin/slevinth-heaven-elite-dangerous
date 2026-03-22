using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class EngineerContributionEvent : EventBase
{
    [JsonPropertyName("Engineer")]
    public string Engineer { get; set; } = string.Empty;

    [JsonPropertyName("EngineerID")]
    public long EngineerID { get; set; }

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Commodity")]
    public string Commodity { get; set; } = string.Empty;

    [JsonPropertyName("Commodity_Localised")]
    public string Commodity_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("TotalQuantity")]
    public int TotalQuantity { get; set; }
}
