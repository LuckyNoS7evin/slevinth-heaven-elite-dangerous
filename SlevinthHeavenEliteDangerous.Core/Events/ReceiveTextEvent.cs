using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class ReceiveTextEvent : EventBase
{
    [JsonPropertyName("From")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("From_Localised")]
    public string From_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("Message_Localised")]
    public string Message_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Channel")]
    public string Channel { get; set; } = string.Empty;
}
