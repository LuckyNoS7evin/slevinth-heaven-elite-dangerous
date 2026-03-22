using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class LandingPads
{
    [JsonPropertyName("Small")]
    public int Small { get; set; }

    [JsonPropertyName("Medium")]
    public int Medium { get; set; }

    [JsonPropertyName("Large")]
    public int Large { get; set; }
}
