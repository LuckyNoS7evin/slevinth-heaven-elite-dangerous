using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class PowerplayFastTrackEvent : EventBase
{
    [JsonPropertyName("Power")]
    public string Power { get; set; } = string.Empty;

    [JsonPropertyName("Cost")]
    public long Cost { get; set; } = 0;
}
