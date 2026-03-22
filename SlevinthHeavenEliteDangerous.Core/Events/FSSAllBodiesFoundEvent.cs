using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class FSSAllBodiesFoundEvent : EventBase
{
    [JsonPropertyName("SystemName")]
    public string SystemName { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("Count")]
    public int Count { get; set; }
}
