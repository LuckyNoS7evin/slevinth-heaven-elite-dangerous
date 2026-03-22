using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class ProspectedAsteroidEvent : EventBase
{
    [JsonPropertyName("Materials")]
    public List<MaterialPercent> Materials { get; set; } = [];

    [JsonPropertyName("Content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("MotherlodeMaterial")]
    public string MotherlodeMaterial { get; set; } = string.Empty;

    [JsonPropertyName("MotherlodeMaterial_Localised")]
    public string MotherlodeMaterial_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Remaining")]
    public double? Remaining { get; set; }
}
