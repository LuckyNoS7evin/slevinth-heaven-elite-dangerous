using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class USSDrop : EventBase
{
    [JsonPropertyName("USSType")]
    public string USSType { get; set; } = string.Empty;

    [JsonPropertyName("USSType_Localised")]
    public string USSType_Localised { get; set; } = string.Empty;

    [JsonPropertyName("USSThreat")]
    public int? USSThreat { get; set; }
}
