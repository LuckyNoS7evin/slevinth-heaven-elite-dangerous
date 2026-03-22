using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.Events;

public class LoadoutEquipModuleEvent : EventBase
{
    [JsonPropertyName("LoadoutName")]
    public string LoadoutName { get; set; } = string.Empty;

    [JsonPropertyName("SuitID")]
    public long SuitID { get; set; }

    [JsonPropertyName("SuitName")]
    public string SuitName { get; set; } = string.Empty;

    [JsonPropertyName("SuitName_Localised")]
    public string SuitName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("LoadoutID")]
    public long LoadoutID { get; set; }

    [JsonPropertyName("SlotName")]
    public string SlotName { get; set; } = string.Empty;

    [JsonPropertyName("ModuleName")]
    public string ModuleName { get; set; } = string.Empty;

    [JsonPropertyName("ModuleName_Localised")]
    public string ModuleName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Class")]
    public int Class { get; set; }

    [JsonPropertyName("WeaponMods")]
    public List<string> WeaponMods { get; set; } = new List<string>();

    [JsonPropertyName("SuitModuleID")]
    public long SuitModuleID { get; set; }
}
