using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class NpcCrewRankEvent : EventBase
{
    [JsonPropertyName("NpcCrewId")]
    public long? NpcCrewId { get; set; }

    [JsonPropertyName("NpcCrewName")]
    public string NpcCrewName { get; set; } = string.Empty;

    [JsonPropertyName("RankCombat")]
    public int? RankCombat { get; set; }
}
