using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SupercruiseDestinationDropEvent : EventBase
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Type_Localised")]
    public string Type_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Threat")]
    public int Threat { get; set; }

    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }
}
