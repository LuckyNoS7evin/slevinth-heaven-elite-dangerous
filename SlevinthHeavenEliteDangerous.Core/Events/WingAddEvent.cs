using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class WingAddEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}
