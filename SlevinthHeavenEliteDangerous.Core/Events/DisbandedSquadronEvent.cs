using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class DisbandedSquadronEvent : EventBase
{
    [JsonPropertyName("SquadronName")]
    public string SquadronName { get; set; } = string.Empty;
}
