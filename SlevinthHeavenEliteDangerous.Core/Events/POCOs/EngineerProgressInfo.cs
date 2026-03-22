using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class EngineerProgressInfo
{
    [JsonPropertyName("Engineer")]
    public string Engineer { get; set; } = string.Empty;

    [JsonPropertyName("EngineerID")]
    public long? EngineerID { get; set; }

    [JsonPropertyName("Progress")]
    public string Progress { get; set; } = string.Empty;
}
