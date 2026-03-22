using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class ReputationEvent : EventBase
{
    [JsonPropertyName("Empire")]
    public double Empire { get; set; }

    [JsonPropertyName("Federation")]
    public double Federation { get; set; }

    [JsonPropertyName("Independent")]
    public double Independent { get; set; }

    [JsonPropertyName("Alliance")]
    public double Alliance { get; set; }
}
