using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SellWeaponEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Price")]
    public long? Price { get; set; }

    [JsonPropertyName("SuitModuleID")]
    public long? SuitModuleID { get; set; }

    [JsonPropertyName("Class")]
    public int? Class { get; set; }

    [JsonPropertyName("WeaponMods")]
    public List<string> WeaponMods { get; set; } = [];
}
