using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class RebootRepairEvent : EventBase
{
    [JsonPropertyName("Modules")]
    public List<string> Modules { get; set; } = [];
}
