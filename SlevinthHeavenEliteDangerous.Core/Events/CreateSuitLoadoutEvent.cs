using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CreateSuitLoadoutEvent : EventBase
{
    [JsonPropertyName("SuitID")]
    public long? SuitID { get; set; }

    [JsonPropertyName("SuitName")]
    public string SuitName { get; set; } = string.Empty;

    [JsonPropertyName("SuitName_Localised")]
    public string SuitName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SuitMods")]
    public List<string> SuitMods { get; set; } = [];

    [JsonPropertyName("LoadoutID")]
    public long? LoadoutID { get; set; }

    [JsonPropertyName("LoadoutName")]
    public string LoadoutName { get; set; } = string.Empty;

    [JsonPropertyName("Modules")]
    public List<string> Modules { get; set; } = [];
}
