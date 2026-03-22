using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class RankEvent : EventBase
{
    [JsonPropertyName("Combat")]
    public int Combat { get; set; }

    [JsonPropertyName("Trade")]
    public int Trade { get; set; }

    [JsonPropertyName("Explore")]
    public int Explore { get; set; }

    [JsonPropertyName("Soldier")]
    public int Soldier { get; set; }

    [JsonPropertyName("Exobiologist")]
    public int Exobiologist { get; set; }

    [JsonPropertyName("Empire")]
    public int Empire { get; set; }

    [JsonPropertyName("Federation")]
    public int Federation { get; set; }

    [JsonPropertyName("CQC")]
    public int CQC { get; set; }
}
