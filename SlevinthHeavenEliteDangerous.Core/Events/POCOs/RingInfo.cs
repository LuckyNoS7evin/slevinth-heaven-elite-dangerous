using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class RingInfo
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("RingClass")]
    public string RingClass { get; set; } = string.Empty;

    [JsonPropertyName("MassMT")]
    public double? MassMT { get; set; }

    [JsonPropertyName("InnerRad")]
    public double? InnerRad { get; set; }

    [JsonPropertyName("OuterRad")]
    public double? OuterRad { get; set; }
}
