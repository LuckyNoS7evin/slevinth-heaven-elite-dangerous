using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class LaunchDroneEvent : EventBase
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;
}
