using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierStatsEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("Callsign")]
    public string Callsign { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("DockingAccess")]
    public string DockingAccess { get; set; } = string.Empty;

    [JsonPropertyName("AllowNotorious")]
    public bool AllowNotorious { get; set; } = false;

    [JsonPropertyName("FuelLevel")]
    public int? FuelLevel { get; set; }

    [JsonPropertyName("JumpRangeCurr")]
    public double? JumpRangeCurr { get; set; }

    [JsonPropertyName("JumpRangeMax")]
    public double? JumpRangeMax { get; set; }

    [JsonPropertyName("PendingDecommission")]
    public bool? PendingDecommission { get; set; }

    [JsonPropertyName("SpaceUsage")]
    public CarrierSpaceUsage? SpaceUsage { get; set; }

    [JsonPropertyName("Finance")]
    public CarrierFinanceInfo? Finance { get; set; }

    [JsonPropertyName("Crew")]
    public List<CarrierCrewMember> Crew { get; set; } = [];

    [JsonPropertyName("ShipPacks")]
    public List<CarrierPackEntry> ShipPacks { get; set; } = [];

    [JsonPropertyName("ModulePacks")]
    public List<CarrierPackEntry> ModulePacks { get; set; } = [];
}
