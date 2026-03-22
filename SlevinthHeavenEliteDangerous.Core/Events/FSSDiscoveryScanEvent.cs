using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class FSSDiscoveryScanEvent : EventBase
{
    [JsonPropertyName("Progress")]
    public double Progress { get; set; }

    [JsonPropertyName("BodyCount")]
    public int BodyCount { get; set; }

    [JsonPropertyName("NonBodyCount")]
    public int NonBodyCount { get; set; }

    [JsonPropertyName("SystemName")]
    public string SystemName { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }
}
