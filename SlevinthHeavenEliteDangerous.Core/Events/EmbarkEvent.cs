using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class EmbarkEvent : EventBase
{
    [JsonPropertyName("SRV")]
    public bool? SRV { get; set; }

    [JsonPropertyName("Taxi")]
    public bool? Taxi { get; set; }

    [JsonPropertyName("Multicrew")]
    public bool? Multicrew { get; set; }

    [JsonPropertyName("ID")]
    public int? ID { get; set; }

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("Body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("BodyID")]
    public int? BodyID { get; set; }

    [JsonPropertyName("OnStation")]
    public bool? OnStation { get; set; }

    [JsonPropertyName("OnPlanet")]
    public bool? OnPlanet { get; set; }

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("StationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("StationType")]
    public string StationType { get; set; } = string.Empty;
}
