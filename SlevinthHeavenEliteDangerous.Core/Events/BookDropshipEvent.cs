using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class BookDropshipEvent : EventBase
{
    [JsonPropertyName("Cost")]
    public long? Cost { get; set; }

    [JsonPropertyName("DestinationSystem")]
    public string DestinationSystem { get; set; } = string.Empty;

    [JsonPropertyName("DestinationLocation")]
    public string DestinationLocation { get; set; } = string.Empty;

    [JsonPropertyName("Retreat")]
    public bool? Retreat { get; set; }
}
