using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class DiscoveredSystem
{
    [JsonPropertyName("SystemName")]
    public string SystemName { get; set; } = string.Empty;

    [JsonPropertyName("NumBodies")]
    public int NumBodies { get; set; }
}
