using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class LeaveBodyEvent : EventBase
{
    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("Body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("BodyID")]
    public int? BodyID { get; set; }
}
