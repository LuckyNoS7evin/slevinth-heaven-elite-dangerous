using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SendTextEvent : EventBase
{
    [JsonPropertyName("To")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("Message")]
    public string Message { get; set; } = string.Empty;
}
