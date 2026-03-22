using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class WingInviteEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}
