using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class RestockVehicleEvent : EventBase
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Type_Localised")]
    public string Type_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Loadout")]
    public string Loadout { get; set; } = string.Empty;

    [JsonPropertyName("Cost")]
    public long? Cost { get; set; }

    [JsonPropertyName("Count")]
    public int? Count { get; set; }

    [JsonPropertyName("ID")]
    public int? ID { get; set; }
}
