using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class PowerplayVoucherEvent : EventBase
{
    [JsonPropertyName("Power")]
    public string Power { get; set; } = string.Empty;

    [JsonPropertyName("Systems")]
    public List<string> Systems { get; set; } = [];
}
