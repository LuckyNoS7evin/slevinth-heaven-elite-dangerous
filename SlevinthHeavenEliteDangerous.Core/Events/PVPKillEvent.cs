using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class PVPKillEvent : EventBase
{
    [JsonPropertyName("Victim")]
    public string Victim { get; set; } = string.Empty;

    [JsonPropertyName("CombatRank")]
    public int CombatRank { get; set; } = 0;
}
