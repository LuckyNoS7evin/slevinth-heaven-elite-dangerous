using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class FSDTargetEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("StarClass")]
    public string StarClass { get; set; } = string.Empty;

    [JsonPropertyName("RemainingJumpsInRoute")]
    public int? RemainingJumpsInRoute { get; set; }
}
