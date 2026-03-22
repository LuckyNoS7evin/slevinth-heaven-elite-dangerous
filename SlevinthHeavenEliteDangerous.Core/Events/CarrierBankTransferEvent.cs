using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierBankTransferEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("Deposit")]
    public long? Deposit { get; set; }

    [JsonPropertyName("Withdraw")]
    public long? Withdraw { get; set; }

    [JsonPropertyName("PlayerBalance")]
    public long? PlayerBalance { get; set; }

    [JsonPropertyName("CarrierBalance")]
    public long? CarrierBalance { get; set; }
}
