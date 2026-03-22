using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class BioDataEntry
{
    [JsonPropertyName("Genus")]
    public string Genus { get; set; } = string.Empty;

    [JsonPropertyName("Genus_Localised")]
    public string Genus_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Species")]
    public string Species { get; set; } = string.Empty;

    [JsonPropertyName("Species_Localised")]
    public string Species_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Variant")]
    public string Variant { get; set; } = string.Empty;

    [JsonPropertyName("Variant_Localised")]
    public string Variant_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Value")]
    public long Value { get; set; }

    [JsonPropertyName("Bonus")]
    public long Bonus { get; set; }
}
