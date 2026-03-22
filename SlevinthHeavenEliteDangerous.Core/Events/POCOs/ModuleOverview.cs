using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class ModuleOverview
{
    // covers both Loadout and SuitLoadout module shapes
    [JsonPropertyName("Slot")]
    public string Slot { get; set; } = string.Empty;

    [JsonPropertyName("SlotName")]
    public string SlotName { get; set; } = string.Empty;

    [JsonPropertyName("Item")]
    public string Item { get; set; } = string.Empty;

    [JsonPropertyName("ModuleName")]
    public string ModuleName { get; set; } = string.Empty;

    [JsonPropertyName("ModuleName_Localised")]
    public string ModuleName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("On")]
    public bool? On { get; set; }

    [JsonPropertyName("Priority")]
    public int? Priority { get; set; }

    [JsonPropertyName("AmmoInClip")]
    public int? AmmoInClip { get; set; }

    [JsonPropertyName("AmmoInHopper")]
    public int? AmmoInHopper { get; set; }

    [JsonPropertyName("Health")]
    public double? Health { get; set; }

    [JsonPropertyName("Value")]
    public long? Value { get; set; }

    [JsonPropertyName("SuitModuleID")]
    public long? SuitModuleID { get; set; }

    [JsonPropertyName("Class")]
    public int? Class { get; set; }

    [JsonPropertyName("WeaponMods")]
    public List<object> WeaponMods { get; set; } = new();
}
