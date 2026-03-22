using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class PowerplayDefectEvent : EventBase
{
    [JsonPropertyName("FromPower")]
    public string FromPower { get; set; } = string.Empty;

    [JsonPropertyName("ToPower")]
    public string ToPower { get; set; } = string.Empty;
}
