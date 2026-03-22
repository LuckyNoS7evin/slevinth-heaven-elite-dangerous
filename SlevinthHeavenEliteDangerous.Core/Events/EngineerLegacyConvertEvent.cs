using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class EngineerLegacyConvertEvent : EventBase
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

    [JsonPropertyName("IsPreview")]
    public bool IsPreview { get; set; } = false;

    [JsonPropertyName("Modifiers")]
    public List<EngineerModifier> Modifiers { get; set; } = [];
}
