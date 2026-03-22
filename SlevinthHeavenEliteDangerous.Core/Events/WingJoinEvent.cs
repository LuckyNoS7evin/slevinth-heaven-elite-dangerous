using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class WingJoinEvent : EventBase
{
    [JsonPropertyName("Others")]
    public List<string> Others { get; set; } = [];
}
