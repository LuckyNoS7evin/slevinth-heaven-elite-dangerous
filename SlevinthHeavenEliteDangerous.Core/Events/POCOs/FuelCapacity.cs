using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class FuelCapacity
{
    [JsonPropertyName("Main")]
    public double? Main { get; set; }

    [JsonPropertyName("Reserve")]
    public double? Reserve { get; set; }
}
