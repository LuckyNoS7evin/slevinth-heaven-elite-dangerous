using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class GenusEntry
{
    [JsonPropertyName("Genus")]
    public string Genus { get; set; } = string.Empty;

    [JsonPropertyName("Genus_Localised")]
    public string Genus_Localised { get; set; } = string.Empty;
}
