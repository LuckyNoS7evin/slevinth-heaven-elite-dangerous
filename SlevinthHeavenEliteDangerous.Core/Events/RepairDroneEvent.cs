using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class RepairDroneEvent : EventBase
{
    [JsonPropertyName("HullRepaired")]
    public double? HullRepaired { get; set; }

    [JsonPropertyName("CockpitRepaired")]
    public double? CockpitRepaired { get; set; }

    [JsonPropertyName("CorrosionRepaired")]
    public double? CorrosionRepaired { get; set; }
}
