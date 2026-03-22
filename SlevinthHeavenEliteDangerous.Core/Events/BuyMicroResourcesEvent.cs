using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class BuyMicroResourcesEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int? Count { get; set; }

    [JsonPropertyName("Price")]
    public long? Price { get; set; }

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }
}
