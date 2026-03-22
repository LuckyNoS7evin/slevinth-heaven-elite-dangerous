using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class FetchRemoteModuleEvent : EventBase
{
    [JsonPropertyName("StorageSlot")]
    public int StorageSlot { get; set; }

    [JsonPropertyName("StoredItem")]
    public string StoredItem { get; set; } = string.Empty;

    [JsonPropertyName("StoredItem_Localised")]
    public string StoredItem_Localised { get; set; } = string.Empty;

    [JsonPropertyName("ServerId")]
    public long ServerId { get; set; }

    [JsonPropertyName("TransferCost")]
    public long TransferCost { get; set; }

    [JsonPropertyName("TransferTime")]
    public int TransferTime { get; set; }

    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int ShipID { get; set; }
}
