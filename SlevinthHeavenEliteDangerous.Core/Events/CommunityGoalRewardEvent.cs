using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CommunityGoalRewardEvent : EventBase
{
    [JsonPropertyName("CGID")]
    public int CGID { get; set; } = 0;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("System")]
    public string System { get; set; } = string.Empty;

    [JsonPropertyName("Reward")]
    public long Reward { get; set; } = 0;
}
