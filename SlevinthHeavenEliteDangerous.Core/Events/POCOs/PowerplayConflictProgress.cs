using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class PowerplayConflictProgress
{
    [JsonPropertyName("Power")]
    public string Power { get; set; } = string.Empty;
    [JsonPropertyName("ConflictProgress")]
    public double ConflictProgress { get; set; }
}
