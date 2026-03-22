using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class FriendsEvent : EventBase
{
    [JsonPropertyName("Status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}
