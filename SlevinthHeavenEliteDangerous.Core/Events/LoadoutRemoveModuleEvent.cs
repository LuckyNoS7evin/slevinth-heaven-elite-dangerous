using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class LoadoutRemoveModuleEvent : EventBase
{
    [JsonPropertyName("SuitID")]
    public long? SuitID { get; set; }

    [JsonPropertyName("SuitName")]
    public string SuitName { get; set; } = string.Empty;

    [JsonPropertyName("SuitName_Localised")]
    public string SuitName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SlotName")]
    public string SlotName { get; set; } = string.Empty;

    [JsonPropertyName("LoadoutID")]
    public long? LoadoutID { get; set; }

    [JsonPropertyName("LoadoutName")]
    public string LoadoutName { get; set; } = string.Empty;

    [JsonPropertyName("ModuleName")]
    public string ModuleName { get; set; } = string.Empty;

    [JsonPropertyName("ModuleName_Localised")]
    public string ModuleName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SuitModuleID")]
    public long? SuitModuleID { get; set; }

    [JsonPropertyName("Class")]
    public int? Class { get; set; }

    [JsonPropertyName("WeaponMods")]
    public List<string> WeaponMods { get; set; } = [];
}
