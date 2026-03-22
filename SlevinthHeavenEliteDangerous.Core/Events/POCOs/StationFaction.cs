using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class StationFaction
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}
