using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class CargoDepotEvent : EventBase
{
    [JsonPropertyName("MissionID")]
    public long MissionID { get; set; }

    [JsonPropertyName("UpdateType")]
    public string UpdateType { get; set; } = string.Empty;

    [JsonPropertyName("CargoType")]
    public string CargoType { get; set; } = string.Empty;

    [JsonPropertyName("CargoType_Localised")]
    public string CargoType_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("StartMarketID")]
    public long? StartMarketID { get; set; }

    [JsonPropertyName("EndMarketID")]
    public long? EndMarketID { get; set; }

    [JsonPropertyName("ItemsCollected")]
    public int ItemsCollected { get; set; }

    [JsonPropertyName("ItemsDelivered")]
    public int ItemsDelivered { get; set; }

    [JsonPropertyName("TotalItemsToDeliver")]
    public int TotalItemsToDeliver { get; set; }

    [JsonPropertyName("Progress")]
    public double Progress { get; set; }
}
