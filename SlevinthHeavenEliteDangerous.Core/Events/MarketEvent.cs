using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class MarketEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("StationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("StationType")]
    public string StationType { get; set; } = string.Empty;

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("CarrierDockingAccess")]
    public string CarrierDockingAccess { get; set; } = string.Empty;
}
