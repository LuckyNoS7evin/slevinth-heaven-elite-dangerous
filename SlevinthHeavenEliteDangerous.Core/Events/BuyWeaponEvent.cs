using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.Events;

public class BuyWeaponEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Class")]
    public int Class { get; set; }

    [JsonPropertyName("Price")]
    public long Price { get; set; }

    [JsonPropertyName("SuitModuleID")]
    public long SuitModuleID { get; set; }

    [JsonPropertyName("WeaponMods")]
    public List<string> WeaponMods { get; set; } = new List<string>();
}
