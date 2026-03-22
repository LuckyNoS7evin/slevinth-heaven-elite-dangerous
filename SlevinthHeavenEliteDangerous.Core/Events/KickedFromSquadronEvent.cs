using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class KickedFromSquadronEvent : EventBase
{
    [JsonPropertyName("SquadronName")]
    public string SquadronName { get; set; } = string.Empty;
}
