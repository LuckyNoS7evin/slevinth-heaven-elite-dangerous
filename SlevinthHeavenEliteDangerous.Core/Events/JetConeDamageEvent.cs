using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class JetConeDamageEvent : EventBase
{
    [JsonPropertyName("Module")]
    public string Module { get; set; } = string.Empty;
}
