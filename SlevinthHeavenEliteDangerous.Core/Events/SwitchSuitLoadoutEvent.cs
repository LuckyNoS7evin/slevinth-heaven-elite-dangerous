using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SwitchSuitLoadoutEvent : EventBase
{
    [JsonPropertyName("SuitID")]
    public long? SuitID { get; set; }

    [JsonPropertyName("SuitName")]
    public string SuitName { get; set; } = string.Empty;

    [JsonPropertyName("SuitName_Localised")]
    public string SuitName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("LoadoutID")]
    public long? LoadoutID { get; set; }

    [JsonPropertyName("LoadoutName")]
    public string LoadoutName { get; set; } = string.Empty;

    [JsonPropertyName("SuitMods")]
    public List<object> SuitMods { get; set; } = [];

    [JsonPropertyName("Modules")]
    public List<object> Modules { get; set; } = [];
}
