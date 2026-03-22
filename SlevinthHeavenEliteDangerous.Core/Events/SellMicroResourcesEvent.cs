using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class SellMicroResourcesEvent : EventBase
{
    [JsonPropertyName("MicroResources")]
    public List<ManufacturedItem> MicroResources { get; set; } = [];

    [JsonPropertyName("Price")]
    public long? Price { get; set; }

    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }
}
