using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class FactionEffect
{
    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("Effects")]
    public List<object> Effects { get; set; } = new();

    [JsonPropertyName("Influence")]
    public List<InfluenceEntry> Influence { get; set; } = new();

    [JsonPropertyName("ReputationTrend")]
    public string ReputationTrend { get; set; } = string.Empty;

    [JsonPropertyName("Reputation")]
    public string Reputation { get; set; } = string.Empty;
}
