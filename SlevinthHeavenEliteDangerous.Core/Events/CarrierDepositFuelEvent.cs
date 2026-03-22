using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierDepositFuelEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("Amount")]
    public int? Amount { get; set; }

    [JsonPropertyName("Total")]
    public int? Total { get; set; }
}
