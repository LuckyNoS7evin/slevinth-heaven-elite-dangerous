using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class MissionRedirectedEvent : EventBase
{
    [JsonPropertyName("MissionID")]
    public long? MissionID { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("LocalisedName")]
    public string LocalisedName { get; set; } = string.Empty;

    [JsonPropertyName("NewDestinationStation")]
    public string NewDestinationStation { get; set; } = string.Empty;

    [JsonPropertyName("NewDestinationSystem")]
    public string NewDestinationSystem { get; set; } = string.Empty;

    [JsonPropertyName("OldDestinationStation")]
    public string OldDestinationStation { get; set; } = string.Empty;

    [JsonPropertyName("OldDestinationSystem")]
    public string OldDestinationSystem { get; set; } = string.Empty;
}
