using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ReservoirReplenishedEvent : EventBase
{
    [JsonPropertyName("FuelMain")]
    public double? FuelMain { get; set; }

    [JsonPropertyName("FuelReservoir")]
    public double? FuelReservoir { get; set; }
}
