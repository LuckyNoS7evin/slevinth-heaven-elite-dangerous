using System.Text.Json.Serialization;
using System.Collections.Generic;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class SuitLoadoutEvent : EventBase
{
    [JsonPropertyName("SuitID")]
    public long? SuitID { get; set; }

    [JsonPropertyName("SuitName")]
    public string SuitName { get; set; } = string.Empty;

    [JsonPropertyName("SuitName_Localised")]
    public string SuitName_Localised { get; set; } = string.Empty;

    // Suit mods are represented as a list of strings in journal snapshots
    [JsonPropertyName("SuitMods")]
    public List<string> SuitMods { get; set; } = [];

    [JsonPropertyName("LoadoutID")]
    public long? LoadoutID { get; set; }

    [JsonPropertyName("LoadoutName")]
    public string LoadoutName { get; set; } = string.Empty;

    [JsonPropertyName("Modules")]
    public List<ModuleOverview> Modules { get; set; } = [];
}
