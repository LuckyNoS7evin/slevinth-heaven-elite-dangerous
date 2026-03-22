using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class StoredModule
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("StorageSlot")]
    public int StorageSlot { get; set; }

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("TransferCost")]
    public long TransferCost { get; set; }

    [JsonPropertyName("TransferTime")]
    public int TransferTime { get; set; }

    [JsonPropertyName("BuyPrice")]
    public long BuyPrice { get; set; }

    [JsonPropertyName("Hot")]
    public bool Hot { get; set; }

    [JsonPropertyName("InTransit")]
    public bool? InTransit { get; set; }
}
