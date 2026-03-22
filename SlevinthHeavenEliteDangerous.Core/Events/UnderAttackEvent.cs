using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class UnderAttackEvent : EventBase
{
    [JsonPropertyName("Target")]
    public string Target { get; set; } = string.Empty;
}
