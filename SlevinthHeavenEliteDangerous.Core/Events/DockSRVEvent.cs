using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class DockSRVEvent : EventBase
{
    [JsonPropertyName("SRVType")]
    public string SRVType { get; set; } = string.Empty;

    [JsonPropertyName("SRVType_Localised")]
    public string SRVType_Localised { get; set; } = string.Empty;

    [JsonPropertyName("ID")]
    public int ID { get; set; }
}
