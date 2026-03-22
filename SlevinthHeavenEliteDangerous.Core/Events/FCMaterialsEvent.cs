using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class FCMaterialsEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("CarrierName")]
    public string CarrierName { get; set; } = string.Empty;

    [JsonPropertyName("CarrierID")]
    public string CarrierID { get; set; } = string.Empty;
}
