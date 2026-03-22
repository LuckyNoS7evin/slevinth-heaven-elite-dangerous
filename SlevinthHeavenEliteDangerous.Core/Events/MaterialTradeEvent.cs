using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class MaterialTradeEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("TraderType")]
    public string TraderType { get; set; } = string.Empty;

    [JsonPropertyName("Paid")]
    public MaterialTradeInfo? Paid { get; set; }

    [JsonPropertyName("Received")]
    public MaterialTradeInfo? Received { get; set; }
}
