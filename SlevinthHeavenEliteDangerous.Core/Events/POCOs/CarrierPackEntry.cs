using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CarrierPackEntry
{
    [JsonPropertyName("PackTheme")]
    public string PackTheme { get; set; } = string.Empty;

    [JsonPropertyName("PackTier")]
    public int? PackTier { get; set; }
}
