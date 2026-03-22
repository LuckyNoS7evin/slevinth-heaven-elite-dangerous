using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class InterdictionEvent : EventBase
{
    [JsonPropertyName("Success")]
    public bool Success { get; set; } = false;

    [JsonPropertyName("Interdicted")]
    public string Interdicted { get; set; } = string.Empty;

    [JsonPropertyName("IsPlayer")]
    public bool IsPlayer { get; set; } = false;

    [JsonPropertyName("CombatRank")]
    public int? CombatRank { get; set; }

    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("Power")]
    public string Power { get; set; } = string.Empty;
}
