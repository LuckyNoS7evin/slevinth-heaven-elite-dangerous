using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class EjectCargoEvent : EventBase
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Type_Localised")]
    public string Type_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int? Count { get; set; }

    [JsonPropertyName("Abandoned")]
    public bool? Abandoned { get; set; }

    [JsonPropertyName("MissionID")]
    public long? MissionID { get; set; }
}
