using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class VehicleSwitchEvent : EventBase
{
    [JsonPropertyName("To")]
    public string To { get; set; } = string.Empty;
}
