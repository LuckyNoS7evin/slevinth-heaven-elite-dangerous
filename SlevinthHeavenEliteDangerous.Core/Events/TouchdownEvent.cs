using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class TouchdownEvent : EventBase
{
    [JsonPropertyName("PlayerControlled")]
    public bool? PlayerControlled { get; set; }

    [JsonPropertyName("Taxi")]
    public bool? Taxi { get; set; }

    [JsonPropertyName("Multicrew")]
    public bool? Multicrew { get; set; }

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

    [JsonPropertyName("NearestDestination")]
    public string NearestDestination { get; set; } = string.Empty;

    [JsonPropertyName("NearestDestination_Localised")]
    public string NearestDestination_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("Longitude")]
    public double? Longitude { get; set; }
}
