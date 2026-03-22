using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class RedeemVoucherEvent : EventBase
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Amount")]
    public long Amount { get; set; }

    [JsonPropertyName("BrokerPercentage")]
    public double? BrokerPercentage { get; set; }
}
