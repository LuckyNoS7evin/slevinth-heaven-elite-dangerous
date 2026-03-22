using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CarrierFinanceInfo
{
    [JsonPropertyName("CarrierBalance")]
    public long CarrierBalance { get; set; } = 0;

    [JsonPropertyName("ReserveBalance")]
    public long ReserveBalance { get; set; } = 0;

    [JsonPropertyName("AvailableBalance")]
    public long AvailableBalance { get; set; } = 0;

    [JsonPropertyName("ReservePercent")]
    public double ReservePercent { get; set; } = 0;

    [JsonPropertyName("TaxRate")]
    public double? TaxRate { get; set; }
}
