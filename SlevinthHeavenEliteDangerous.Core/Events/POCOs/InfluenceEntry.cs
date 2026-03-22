using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class InfluenceEntry
{
    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("Trend")]
    public string Trend { get; set; } = string.Empty;

    [JsonPropertyName("Influence")]
    public string Influence { get; set; } = string.Empty;
}
