using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CommunityGoalEntry
{
    [JsonPropertyName("CGID")]
    public int? CGID { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("SystemName")]
    public string SystemName { get; set; } = string.Empty;

    [JsonPropertyName("MarketName")]
    public string MarketName { get; set; } = string.Empty;

    [JsonPropertyName("Expiry")]
    public string Expiry { get; set; } = string.Empty;

    [JsonPropertyName("IsComplete")]
    public bool IsComplete { get; set; } = false;

    [JsonPropertyName("CurrentTotal")]
    public long CurrentTotal { get; set; } = 0;

    [JsonPropertyName("PlayerContribution")]
    public long PlayerContribution { get; set; } = 0;

    [JsonPropertyName("NumContributors")]
    public int NumContributors { get; set; } = 0;

    [JsonPropertyName("PlayerPercentileBand")]
    public int PlayerPercentileBand { get; set; } = 0;

    [JsonPropertyName("TopRankSize")]
    public int? TopRankSize { get; set; }

    [JsonPropertyName("PlayerInTopRank")]
    public bool? PlayerInTopRank { get; set; }

    [JsonPropertyName("TierReached")]
    public string TierReached { get; set; } = string.Empty;

    [JsonPropertyName("Bonus")]
    public long? Bonus { get; set; }

    [JsonPropertyName("TopTier")]
    public CommunityGoalTopTier? TopTier { get; set; }
}
