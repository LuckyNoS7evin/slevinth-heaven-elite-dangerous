using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class DiedEvent : EventBase
{
    [JsonPropertyName("KillerName")]
    public string KillerName { get; set; } = string.Empty;

    [JsonPropertyName("KillerName_Localised")]
    public string KillerName_Localised { get; set; } = string.Empty;

    [JsonPropertyName("KillerShip")]
    public string KillerShip { get; set; } = string.Empty;

    [JsonPropertyName("KillerRank")]
    public string KillerRank { get; set; } = string.Empty;
}
