using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class LoadoutEvent : EventBase
{
    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int ShipID { get; set; }

    [JsonPropertyName("ShipName")]
    public string ShipName { get; set; } = string.Empty;

    [JsonPropertyName("ShipIdent")]
    public string ShipIdent { get; set; } = string.Empty;

    [JsonPropertyName("HullValue")]
    public long HullValue { get; set; }

    [JsonPropertyName("ModulesValue")]
    public long ModulesValue { get; set; }

    [JsonPropertyName("HullHealth")]
    public double HullHealth { get; set; }

    [JsonPropertyName("Hot")]
    public bool Hot { get; set; }

    [JsonPropertyName("UnladenMass")]
    public double UnladenMass { get; set; }

    [JsonPropertyName("CargoCapacity")]
    public int CargoCapacity { get; set; }

    [JsonPropertyName("MaxJumpRange")]
    public double MaxJumpRange { get; set; }

    [JsonPropertyName("FuelCapacity")]
    public FuelCapacity FuelCapacity { get; set; } = new();

    [JsonPropertyName("Rebuy")]
    public long Rebuy { get; set; }

    [JsonPropertyName("Modules")]
    public List<ModuleOverview> Modules { get; set; } = [];
}
