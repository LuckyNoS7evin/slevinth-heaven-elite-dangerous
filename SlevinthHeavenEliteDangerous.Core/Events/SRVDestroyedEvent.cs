using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SRVDestroyedEvent : EventBase
{
    [JsonPropertyName("ID")]
    public int? ID { get; set; }

    [JsonPropertyName("SRVType")]
    public string SRVType { get; set; } = string.Empty;
}
