using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class PayLegacyFinesEvent : EventBase
{
    [JsonPropertyName("Amount")]
    public long Amount { get; set; } = 0;

    [JsonPropertyName("BrokerPercentage")]
    public double? BrokerPercentage { get; set; }
}
