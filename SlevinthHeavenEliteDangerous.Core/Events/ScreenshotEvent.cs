using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ScreenshotEvent : EventBase
{
    [JsonPropertyName("Filename")]
    public string Filename { get; set; } = string.Empty;

    [JsonPropertyName("Width")]
    public int? Width { get; set; }

    [JsonPropertyName("Height")]
    public int? Height { get; set; }

    [JsonPropertyName("System")]
    public string System { get; set; } = string.Empty;

    [JsonPropertyName("Body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("Latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("Longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("Altitude")]
    public double? Altitude { get; set; }

    [JsonPropertyName("Heading")]
    public int? Heading { get; set; }
}
