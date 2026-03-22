using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierFinanceEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("TaxRate")]
    public double? TaxRate { get; set; }

    [JsonPropertyName("CarrierBalance")]
    public long? CarrierBalance { get; set; }

    [JsonPropertyName("ReserveBalance")]
    public long? ReserveBalance { get; set; }

    [JsonPropertyName("AvailableBalance")]
    public long? AvailableBalance { get; set; }

    [JsonPropertyName("ReservePercent")]
    public double? ReservePercent { get; set; }
}
