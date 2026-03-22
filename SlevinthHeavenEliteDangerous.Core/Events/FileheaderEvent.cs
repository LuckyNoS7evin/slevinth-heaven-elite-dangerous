using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class FileheaderEvent : EventBase
{
    [JsonPropertyName("part")]
    public int? Part { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("Odyssey")]
    public bool? Odyssey { get; set; }

    [JsonPropertyName("gameversion")]
    public string GameVersion { get; set; } = string.Empty;

    [JsonPropertyName("build")]
    public string Build { get; set; } = string.Empty;
}
