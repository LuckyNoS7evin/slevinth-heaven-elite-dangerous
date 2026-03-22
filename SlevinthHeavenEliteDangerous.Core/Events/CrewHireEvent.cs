using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CrewHireEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("CrewID")]
    public long CrewID { get; set; } = 0;

    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("Cost")]
    public long Cost { get; set; } = 0;

    [JsonPropertyName("CombatRank")]
    public int CombatRank { get; set; } = 0;
}
