using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class FighterRebuiltEvent : EventBase
{
    [JsonPropertyName("Loadout")]
    public string Loadout { get; set; } = string.Empty;

    [JsonPropertyName("ID")]
    public int? ID { get; set; }
}
