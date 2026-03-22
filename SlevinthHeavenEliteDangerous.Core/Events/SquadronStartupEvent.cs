using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SquadronStartupEvent : EventBase
{
    [JsonPropertyName("SquadronName")]
    public string SquadronName { get; set; } = string.Empty;

    [JsonPropertyName("CurrentRank")]
    public int CurrentRank { get; set; } = 0;
}
