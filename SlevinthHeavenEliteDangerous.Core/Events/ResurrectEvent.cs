using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ResurrectEvent : EventBase
{
    [JsonPropertyName("Option")]
    public string Option { get; set; } = string.Empty;

    [JsonPropertyName("Cost")]
    public long? Cost { get; set; }

    [JsonPropertyName("Bankrupt")]
    public bool? Bankrupt { get; set; }
}
