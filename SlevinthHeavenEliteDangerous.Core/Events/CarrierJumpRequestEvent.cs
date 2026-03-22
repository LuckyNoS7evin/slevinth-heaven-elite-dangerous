using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierJumpRequestEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("SystemName")]
    public string SystemName { get; set; } = string.Empty;

    [JsonPropertyName("Body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("BodyID")]
    public int? BodyID { get; set; }

    [JsonPropertyName("DepartureTime")]
    public string DepartureTime { get; set; } = string.Empty;
}
