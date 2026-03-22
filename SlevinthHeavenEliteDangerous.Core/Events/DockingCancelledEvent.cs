using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class DockingCancelledEvent : EventBase
{
    [JsonPropertyName("StationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("StationType")]
    public string StationType { get; set; } = string.Empty;

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }
}
