using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class CommunityGoalEvent : EventBase
{
    [JsonPropertyName("CurrentGoals")]
    public List<CommunityGoalEntry> CurrentGoals { get; set; } = [];
}
