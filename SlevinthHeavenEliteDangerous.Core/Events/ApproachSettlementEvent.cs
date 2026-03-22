using System.Text.Json.Serialization;
using System.Collections.Generic;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class ApproachSettlementEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("StationFaction")]
    public StationFaction StationFaction { get; set; } = new StationFaction();

    [JsonPropertyName("StationGovernment")]
    public string StationGovernment { get; set; } = string.Empty;

    [JsonPropertyName("StationGovernment_Localised")]
    public string StationGovernment_Localised { get; set; } = string.Empty;

    [JsonPropertyName("StationAllegiance")]
    public string StationAllegiance { get; set; } = string.Empty;

    [JsonPropertyName("StationServices")]
    public List<string> StationServices { get; set; } = new List<string>();

    [JsonPropertyName("StationEconomy")]
    public string StationEconomy { get; set; } = string.Empty;

    [JsonPropertyName("StationEconomy_Localised")]
    public string StationEconomy_Localised { get; set; } = string.Empty;

    [JsonPropertyName("StationEconomies")]
    public List<StationEconomy> StationEconomies { get; set; } = new List<StationEconomy>();

    [JsonPropertyName("SystemAddress")]
    public long SystemAddress { get; set; }

    [JsonPropertyName("BodyID")]
    public int BodyID { get; set; }

    [JsonPropertyName("BodyName")]
    public string BodyName { get; set; } = string.Empty;

    [JsonPropertyName("Latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("Longitude")]
    public double Longitude { get; set; }
}
