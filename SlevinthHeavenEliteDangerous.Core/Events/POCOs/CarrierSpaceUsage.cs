using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CarrierSpaceUsage
{
    [JsonPropertyName("TotalCapacity")]
    public long TotalCapacity { get; set; } = 0;

    [JsonPropertyName("Crew")]
    public long Crew { get; set; } = 0;

    [JsonPropertyName("Cargo")]
    public long Cargo { get; set; } = 0;

    [JsonPropertyName("CargoSpaceReserved")]
    public long CargoSpaceReserved { get; set; } = 0;

    [JsonPropertyName("ShipPacks")]
    public long ShipPacks { get; set; } = 0;

    [JsonPropertyName("ModulePacks")]
    public long ModulePacks { get; set; } = 0;

    [JsonPropertyName("FreeSpace")]
    public long FreeSpace { get; set; } = 0;
}
