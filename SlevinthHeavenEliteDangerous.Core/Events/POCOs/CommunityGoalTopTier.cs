using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CommunityGoalTopTier
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Bonus")]
    public string Bonus { get; set; } = string.Empty;
}
