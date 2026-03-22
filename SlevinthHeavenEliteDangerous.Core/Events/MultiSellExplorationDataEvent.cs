using System.Text.Json.Serialization;
using System.Collections.Generic;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class MultiSellExplorationDataEvent : EventBase
{
    [JsonPropertyName("Discovered")]
    public List<DiscoveredSystem> Discovered { get; set; } = new List<DiscoveredSystem>();

    [JsonPropertyName("BaseValue")]
    public long BaseValue { get; set; }

    [JsonPropertyName("Bonus")]
    public long Bonus { get; set; }

    [JsonPropertyName("TotalEarnings")]
    public long TotalEarnings { get; set; }
}
