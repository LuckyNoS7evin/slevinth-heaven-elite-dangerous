using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class EscapeInterdictionEvent : EventBase
{
    [JsonPropertyName("Interdictor")]
    public string Interdictor { get; set; } = string.Empty;

    [JsonPropertyName("IsPlayer")]
    public bool IsPlayer { get; set; }
}
