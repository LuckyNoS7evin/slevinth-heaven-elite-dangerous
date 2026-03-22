using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CrimeVictimEvent : EventBase
{
    [JsonPropertyName("Offender")]
    public string Offender { get; set; } = string.Empty;

    [JsonPropertyName("CrimeType")]
    public string CrimeType { get; set; } = string.Empty;

    [JsonPropertyName("Fine")]
    public long? Fine { get; set; }

    [JsonPropertyName("Bounty")]
    public long? Bounty { get; set; }
}
