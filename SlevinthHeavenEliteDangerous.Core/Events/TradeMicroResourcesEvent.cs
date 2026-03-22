using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class TradeMicroResourcesEvent : EventBase
{
    [JsonPropertyName("Offered")]
    public List<MicroResourceOfferItem> Offered { get; set; } = [];

    [JsonPropertyName("Received")]
    public string Received { get; set; } = string.Empty;

    [JsonPropertyName("Received_Localised")]
    public string Received_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int? Count { get; set; }

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }
}
