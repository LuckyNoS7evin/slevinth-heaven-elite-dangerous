using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class DiscoveryScanEvent : EventBase
{
    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("Bodies")]
    public int? Bodies { get; set; }
}
