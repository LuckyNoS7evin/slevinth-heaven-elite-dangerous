using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class DockingGrantedEvent : EventBase
{
    [JsonPropertyName("LandingPad")]
    public int LandingPad { get; set; }

    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("StationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("StationType")]
    public string StationType { get; set; } = string.Empty;
}
