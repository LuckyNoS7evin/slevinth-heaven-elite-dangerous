using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class DatalinkScanEvent : EventBase
{
    [JsonPropertyName("Message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("Message_Localised")]
    public string Message_Localised { get; set; } = string.Empty;
}
