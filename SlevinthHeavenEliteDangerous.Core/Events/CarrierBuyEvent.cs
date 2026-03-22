using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierBuyEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("BoughtAtMarket")]
    public long? BoughtAtMarket { get; set; }

    [JsonPropertyName("Location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("Price")]
    public long Price { get; set; } = 0;

    [JsonPropertyName("Variant")]
    public string Variant { get; set; } = string.Empty;

    [JsonPropertyName("Callsign")]
    public string Callsign { get; set; } = string.Empty;
}
