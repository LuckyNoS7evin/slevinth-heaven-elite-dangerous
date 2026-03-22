using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CommitCrimeEvent : EventBase
{
    [JsonPropertyName("CrimeType")]
    public string CrimeType { get; set; } = string.Empty;

    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("Victim")]
    public string Victim { get; set; } = string.Empty;

    [JsonPropertyName("Victim_Localised")]
    public string Victim_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Fine")]
    public long? Fine { get; set; }

    [JsonPropertyName("Bounty")]
    public long? Bounty { get; set; }
}
