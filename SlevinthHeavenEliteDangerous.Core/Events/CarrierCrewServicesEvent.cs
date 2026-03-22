using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierCrewServicesEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("CrewRole")]
    public string CrewRole { get; set; } = string.Empty;

    [JsonPropertyName("Operation")]
    public string Operation { get; set; } = string.Empty;

    [JsonPropertyName("CrewName")]
    public string CrewName { get; set; } = string.Empty;
}
