using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CommunityGoalDiscardEvent : EventBase
{
    [JsonPropertyName("CGID")]
    public int CGID { get; set; } = 0;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("System")]
    public string System { get; set; } = string.Empty;
}
