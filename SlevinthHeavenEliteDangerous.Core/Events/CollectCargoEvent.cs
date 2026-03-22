using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CollectCargoEvent : EventBase
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Type_Localised")]
    public string Type_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Stolen")]
    public bool? Stolen { get; set; }

    [JsonPropertyName("MissionID")]
    public long? MissionID { get; set; }
}
