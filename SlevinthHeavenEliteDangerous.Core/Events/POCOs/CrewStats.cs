using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CrewStats
{
    [JsonPropertyName("NpcCrew_TotalWages")]
    public long? NpcCrewTotalWages { get; set; }

    [JsonPropertyName("NpcCrew_Hired")]
    public int? NpcCrewHired { get; set; }

    [JsonPropertyName("NpcCrew_Fired")]
    public int? NpcCrewFired { get; set; }

    [JsonPropertyName("NpcCrew_Died")]
    public int? NpcCrewDied { get; set; }
}
