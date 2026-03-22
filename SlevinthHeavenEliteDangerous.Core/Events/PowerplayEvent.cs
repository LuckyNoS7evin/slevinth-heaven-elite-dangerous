using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class PowerplayEvent : EventBase
{
    [JsonPropertyName("Power")]
    public string Power { get; set; } = string.Empty;

    [JsonPropertyName("Rank")]
    public int Rank { get; set; } = 0;

    [JsonPropertyName("Merits")]
    public int Merits { get; set; } = 0;

    [JsonPropertyName("Votes")]
    public int Votes { get; set; } = 0;

    [JsonPropertyName("TimePledged")]
    public long TimePledged { get; set; } = 0;
}
