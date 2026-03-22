using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class FactionKillBondEvent : EventBase
{
    [JsonPropertyName("Reward")]
    public long Reward { get; set; } = 0;

    [JsonPropertyName("AwardingFaction")]
    public string AwardingFaction { get; set; } = string.Empty;

    [JsonPropertyName("VictimFaction")]
    public string VictimFaction { get; set; } = string.Empty;
}
