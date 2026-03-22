using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class StoredShipInfo
{
    [JsonPropertyName("ShipID")]
    public int ShipID { get; set; }

    [JsonPropertyName("ShipType")]
    public string ShipType { get; set; } = string.Empty;

    [JsonPropertyName("ShipType_Localised")]
    public string ShipType_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("ShipMarketID")]
    public long ShipMarketID { get; set; }

    [JsonPropertyName("TransferPrice")]
    public long TransferPrice { get; set; }

    [JsonPropertyName("TransferTime")]
    public int TransferTime { get; set; }

    [JsonPropertyName("Value")]
    public long Value { get; set; }

    [JsonPropertyName("Hot")]
    public bool Hot { get; set; }
}
