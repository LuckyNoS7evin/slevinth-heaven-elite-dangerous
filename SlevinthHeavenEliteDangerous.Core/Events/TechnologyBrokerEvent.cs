using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class TechnologyBrokerEvent : EventBase
{
    [JsonPropertyName("BrokerType")]
    public string BrokerType { get; set; } = string.Empty;

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("ItemsUnlocked")]
    public List<ManufacturedItem> ItemsUnlocked { get; set; } = [];

    [JsonPropertyName("Commodities")]
    public List<ManufacturedItem> Commodities { get; set; } = [];

    [JsonPropertyName("Materials")]
    public List<CategoryMaterial> Materials { get; set; } = [];
}
