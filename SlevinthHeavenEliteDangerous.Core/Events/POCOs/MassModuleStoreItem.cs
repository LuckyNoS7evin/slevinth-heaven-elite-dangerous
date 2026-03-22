using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class MassModuleStoreItem
{
    [JsonPropertyName("Slot")]
    public string Slot { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Hot")]
    public bool? Hot { get; set; }

    [JsonPropertyName("EngineerModifications")]
    public string EngineerModifications { get; set; } = string.Empty;

    [JsonPropertyName("Level")]
    public int? Level { get; set; }

    [JsonPropertyName("Quality")]
    public double? Quality { get; set; }
}
