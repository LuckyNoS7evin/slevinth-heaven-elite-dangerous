using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class EngineerCraftEvent : EventBase
{
    [JsonPropertyName("Engineer")]
    public string Engineer { get; set; } = string.Empty;

    [JsonPropertyName("EngineerID")]
    public long EngineerID { get; set; } = 0;

    [JsonPropertyName("BlueprintName")]
    public string BlueprintName { get; set; } = string.Empty;

    [JsonPropertyName("BlueprintID")]
    public long? BlueprintID { get; set; }

    [JsonPropertyName("Level")]
    public int Level { get; set; } = 0;

    [JsonPropertyName("Quality")]
    public double? Quality { get; set; }

    [JsonPropertyName("ApplyExperimentalEffect")]
    public string ApplyExperimentalEffect { get; set; } = string.Empty;

    [JsonPropertyName("Slot")]
    public string Slot { get; set; } = string.Empty;

    [JsonPropertyName("Module")]
    public string Module { get; set; } = string.Empty;

    [JsonPropertyName("Module_Localised")]
    public string Module_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Ingredients")]
    public List<ManufacturedItem> Ingredients { get; set; } = [];

    [JsonPropertyName("Modifiers")]
    public List<EngineerModifier> Modifiers { get; set; } = [];
}
