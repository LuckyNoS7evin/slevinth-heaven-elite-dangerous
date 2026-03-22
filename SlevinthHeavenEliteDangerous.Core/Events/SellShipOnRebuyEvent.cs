using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SellShipOnRebuyEvent : EventBase
{
    [JsonPropertyName("ShipType")]
    public string ShipType { get; set; } = string.Empty;

    [JsonPropertyName("System")]
    public string System { get; set; } = string.Empty;

    [JsonPropertyName("SellShipId")]
    public int? SellShipId { get; set; }

    [JsonPropertyName("ShipPrice")]
    public long ShipPrice { get; set; } = 0;
}
