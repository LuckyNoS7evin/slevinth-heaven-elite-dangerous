using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierModulePackEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("Operation")]
    public string Operation { get; set; } = string.Empty;

    [JsonPropertyName("PackTheme")]
    public string PackTheme { get; set; } = string.Empty;

    [JsonPropertyName("PackTier")]
    public int? PackTier { get; set; }

    [JsonPropertyName("Cost")]
    public long? Cost { get; set; }

    [JsonPropertyName("Refund")]
    public long? Refund { get; set; }
}
