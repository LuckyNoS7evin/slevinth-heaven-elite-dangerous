using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SquadronDemotionEvent : EventBase
{
    [JsonPropertyName("SquadronName")]
    public string SquadronName { get; set; } = string.Empty;

    [JsonPropertyName("OldRank")]
    public int OldRank { get; set; } = 0;

    [JsonPropertyName("NewRank")]
    public int NewRank { get; set; } = 0;
}
