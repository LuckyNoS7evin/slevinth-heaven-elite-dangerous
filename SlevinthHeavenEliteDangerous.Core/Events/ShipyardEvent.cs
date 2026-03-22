using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ShipyardEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("StationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;
}
