using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierJumpEvent : EventBase
{
    [JsonPropertyName("Docked")]
    public bool? Docked { get; set; }

    [JsonPropertyName("StationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("StationType")]
    public string StationType { get; set; } = string.Empty;

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("StationFaction")]
    public FactionInfo? StationFaction { get; set; }

    [JsonPropertyName("StationGovernment")]
    public string StationGovernment { get; set; } = string.Empty;

    [JsonPropertyName("StationGovernment_Localised")]
    public string StationGovernment_Localised { get; set; } = string.Empty;

    [JsonPropertyName("StationServices")]
    public List<string> StationServices { get; set; } = [];

    [JsonPropertyName("StationEconomy")]
    public string StationEconomy { get; set; } = string.Empty;

    [JsonPropertyName("StationEconomy_Localised")]
    public string StationEconomy_Localised { get; set; } = string.Empty;

    [JsonPropertyName("StationEconomies")]
    public List<StationEconomy> StationEconomies { get; set; } = [];

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

    [JsonPropertyName("SystemFaction")]
    public FactionInfo? SystemFaction { get; set; }
}
