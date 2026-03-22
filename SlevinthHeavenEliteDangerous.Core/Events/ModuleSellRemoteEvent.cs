using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ModuleSellRemoteEvent : EventBase
{
    [JsonPropertyName("StorageSlot")]
    public int? StorageSlot { get; set; }

    [JsonPropertyName("SellItem")]
    public string SellItem { get; set; } = string.Empty;

    [JsonPropertyName("SellItem_Localised")]
    public string SellItem_Localised { get; set; } = string.Empty;

    [JsonPropertyName("ServerId")]
    public int? ServerId { get; set; }

    [JsonPropertyName("SellPrice")]
    public long? SellPrice { get; set; }

    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int? ShipID { get; set; }
}
