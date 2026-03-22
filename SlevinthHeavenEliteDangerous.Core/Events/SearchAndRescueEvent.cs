using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SearchAndRescueEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int Count { get; set; } = 0;

    [JsonPropertyName("Reward")]
    public long Reward { get; set; } = 0;
}
