using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class AfmuRepairsEvent : EventBase
{
    [JsonPropertyName("Module")]
    public string Module { get; set; } = string.Empty;

    [JsonPropertyName("Module_Localised")]
    public string Module_Localised { get; set; } = string.Empty;

    [JsonPropertyName("FullyRepaired")]
    public bool? FullyRepaired { get; set; }

    [JsonPropertyName("Health")]
    public double? Health { get; set; }
}
