using System.Text.Json.Serialization;
using System.Collections.Generic;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class DockedEvent : EventBase
{
    [JsonPropertyName("StationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("StationType")]
    public string StationType { get; set; } = string.Empty;

    [JsonPropertyName("Taxi")]
    public bool Taxi { get; set; }

    [JsonPropertyName("Multicrew")]
    public bool Multicrew { get; set; }

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long SystemAddress { get; set; }

    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("StationFaction")]
    public StationFaction StationFaction { get; set; } = new StationFaction();

    [JsonPropertyName("StationGovernment")]
    public string StationGovernment { get; set; } = string.Empty;

    [JsonPropertyName("StationGovernment_Localised")]
    public string StationGovernment_Localised { get; set; } = string.Empty;

    [JsonPropertyName("StationServices")]
    public List<string> StationServices { get; set; } = new List<string>();

    [JsonPropertyName("ActiveFine")]
    public bool? ActiveFine { get; set; }

    [JsonPropertyName("StationAllegiance")]
    public string StationAllegiance { get; set; } = string.Empty;

    [JsonPropertyName("Wanted")]
    public bool? Wanted { get; set; }

    [JsonPropertyName("StationEconomy")]
    public string StationEconomy { get; set; } = string.Empty;

    [JsonPropertyName("StationEconomy_Localised")]
    public string StationEconomy_Localised { get; set; } = string.Empty;

    [JsonPropertyName("StationEconomies")]
    public List<StationEconomy> StationEconomies { get; set; } = new List<StationEconomy>();

    [JsonPropertyName("DistFromStarLS")]
    public double DistFromStarLS { get; set; }

    [JsonPropertyName("LandingPads")]
    public LandingPads LandingPads { get; set; } = new LandingPads();
}
