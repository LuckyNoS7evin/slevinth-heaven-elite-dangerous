using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class PowerplayVoteEvent : EventBase
{
    [JsonPropertyName("Power")]
    public string Power { get; set; } = string.Empty;

    [JsonPropertyName("Votes")]
    public int Votes { get; set; } = 0;

    [JsonPropertyName("System")]
    public string System { get; set; } = string.Empty;
}
