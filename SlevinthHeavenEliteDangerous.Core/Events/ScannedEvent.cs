using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ScannedEvent : EventBase
{
    [JsonPropertyName("ScanType")]
    public string ScanType { get; set; } = string.Empty;
}
