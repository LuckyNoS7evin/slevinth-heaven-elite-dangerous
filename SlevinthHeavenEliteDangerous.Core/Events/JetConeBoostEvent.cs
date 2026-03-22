using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class JetConeBoostEvent : EventBase
{
    [JsonPropertyName("BoostValue")]
    public double? BoostValue { get; set; }
}
