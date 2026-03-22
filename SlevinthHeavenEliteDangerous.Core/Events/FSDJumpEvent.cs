using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class FSDJumpEvent : EventBase
{
    [JsonPropertyName("Taxi")]
    public bool? Taxi { get; set; }

    [JsonPropertyName("Multicrew")]
    public bool? Multicrew { get; set; }

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("StarPos")]
    public double[]? StarPos { get; set; }

    [JsonPropertyName("SystemAllegiance")]
    public string SystemAllegiance { get; set; } = string.Empty;

    [JsonPropertyName("SystemEconomy")]
    public string SystemEconomy { get; set; } = string.Empty;

    [JsonPropertyName("SystemEconomy_Localised")]
    public string SystemEconomy_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SystemSecondEconomy")]
    public string SystemSecondEconomy { get; set; } = string.Empty;

    [JsonPropertyName("SystemSecondEconomy_Localised")]
    public string SystemSecondEconomy_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SystemGovernment")]
    public string SystemGovernment { get; set; } = string.Empty;

    [JsonPropertyName("SystemGovernment_Localised")]
    public string SystemGovernment_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SystemSecurity")]
    public string SystemSecurity { get; set; } = string.Empty;

    [JsonPropertyName("SystemSecurity_Localised")]
    public string SystemSecurity_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Population")]
    public long? Population { get; set; }

    [JsonPropertyName("Body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("BodyID")]
    public int? BodyID { get; set; }

    [JsonPropertyName("BodyType")]
    public string BodyType { get; set; } = string.Empty;

    [JsonPropertyName("Powers")]
    public List<string> Powers { get; set; } = [];

    [JsonPropertyName("PowerplayState")]
    public string PowerplayState { get; set; } = string.Empty;

    [JsonPropertyName("ControllingPower")]
    public string ControllingPower { get; set; } = string.Empty;

    [JsonPropertyName("PowerplayConflictProgress")]
    public List<PowerplayConflictProgress?> PowerplayConflictProgress { get; set; } = [];

    [JsonPropertyName("PowerplayStateControlProgress")]
    public double? PowerplayStateControlProgress { get; set; }

    [JsonPropertyName("PowerplayStateReinforcement")]
    public double? PowerplayStateReinforcement { get; set; }

    [JsonPropertyName("PowerplayStateUndermining")]
    public double? PowerplayStateUndermining { get; set; }

    [JsonPropertyName("SystemFaction")]
    public FactionInfo? SystemFaction { get; set; }

    [JsonPropertyName("Conflicts")]
    public List<object> Conflicts { get; set; } = [];

    [JsonPropertyName("JumpDist")]
    public double? JumpDist { get; set; }

    [JsonPropertyName("FuelUsed")]
    public double? FuelUsed { get; set; }

    [JsonPropertyName("FuelLevel")]
    public double? FuelLevel { get; set; }

    [JsonPropertyName("Factions")]
    public List<FactionInfo> Factions { get; set; } = [];
}
