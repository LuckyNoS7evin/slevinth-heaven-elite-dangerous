using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class MassModuleStoreEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long? MarketID { get; set; }

    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("ShipId")]
    public int? ShipId { get; set; }

    [JsonPropertyName("Items")]
    public List<MassModuleStoreItem> Items { get; set; } = [];
}
