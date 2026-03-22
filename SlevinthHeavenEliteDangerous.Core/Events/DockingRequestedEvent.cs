using System.Text.Json.Serialization;
using System;
using System.Text.Json;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class DockingRequestedEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("StationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("StationType")]
    public string StationType { get; set; } = string.Empty;

    [JsonPropertyName("LandingPads")]
    public LandingPadsInfo LandingPads { get; set; } = new();
}
