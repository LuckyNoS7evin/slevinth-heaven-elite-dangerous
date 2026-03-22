using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class PayBountiesEvent : EventBase
{
    [JsonPropertyName("Amount")]
    public long? Amount { get; set; }

    [JsonPropertyName("AllFines")]
    public bool? AllFines { get; set; }

    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("Faction_Localised")]
    public string Faction_Localised { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int? ShipID { get; set; }

    [JsonPropertyName("BrokerPercentage")]
    public double? BrokerPercentage { get; set; }
}
