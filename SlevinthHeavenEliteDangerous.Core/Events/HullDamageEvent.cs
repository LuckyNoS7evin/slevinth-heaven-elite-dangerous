using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class HullDamageEvent : EventBase
{
    [JsonPropertyName("Health")]
    public double? Health { get; set; }

    [JsonPropertyName("PlayerPilot")]
    public bool? PlayerPilot { get; set; }

    [JsonPropertyName("Fighter")]
    public bool? Fighter { get; set; }
}
