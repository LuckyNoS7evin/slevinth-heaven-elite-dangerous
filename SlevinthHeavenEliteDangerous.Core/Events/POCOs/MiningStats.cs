using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class MiningStats
{
    [JsonPropertyName("Mining_Profits")]
    public long? MiningProfits { get; set; }

    [JsonPropertyName("Quantity_Mined")]
    public int? QuantityMined { get; set; }

    [JsonPropertyName("Materials_Collected")]
    public int? MaterialsCollected { get; set; }
}
