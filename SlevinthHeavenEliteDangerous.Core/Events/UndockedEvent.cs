using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class UndockedEvent : EventBase
{
    [JsonPropertyName("StationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("StationType")]
    public string StationType { get; set; } = string.Empty;

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("Taxi")]
    public bool? Taxi { get; set; }

    [JsonPropertyName("Multicrew")]
    public bool? Multicrew { get; set; }
}
