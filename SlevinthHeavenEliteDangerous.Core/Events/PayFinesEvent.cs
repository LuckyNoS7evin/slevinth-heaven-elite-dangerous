using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class PayFinesEvent : EventBase
{
    [JsonPropertyName("Amount")]
    public long Amount { get; set; } = 0;

    [JsonPropertyName("BrokerPercentage")]
    public double? BrokerPercentage { get; set; }

    [JsonPropertyName("AllFines")]
    public bool AllFines { get; set; } = false;

    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int? ShipID { get; set; }
}
