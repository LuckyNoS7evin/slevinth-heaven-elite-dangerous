using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class InterdictedEvent : EventBase
{
    [JsonPropertyName("Submitted")]
    public bool? Submitted { get; set; }

    [JsonPropertyName("Interdictor")]
    public string Interdictor { get; set; } = string.Empty;

    [JsonPropertyName("Interdictor_Localised")]
    public string Interdictor_Localised { get; set; } = string.Empty;

    [JsonPropertyName("IsPlayer")]
    public bool? IsPlayer { get; set; }

    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("Power")]
    public string Power { get; set; } = string.Empty;
}
