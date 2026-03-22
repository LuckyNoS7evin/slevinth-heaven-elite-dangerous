using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SellExplorationDataEvent : EventBase
{
    [JsonPropertyName("Systems")]
    public List<string> Systems { get; set; } = [];

    [JsonPropertyName("Discovered")]
    public List<string> Discovered { get; set; } = [];

    [JsonPropertyName("BaseValue")]
    public long BaseValue { get; set; } = 0;

    [JsonPropertyName("Bonus")]
    public long Bonus { get; set; } = 0;

    [JsonPropertyName("TotalEarnings")]
    public long TotalEarnings { get; set; } = 0;
}
