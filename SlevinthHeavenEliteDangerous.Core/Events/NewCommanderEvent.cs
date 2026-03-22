using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class NewCommanderEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("FID")]
    public string FID { get; set; } = string.Empty;

    [JsonPropertyName("Package")]
    public string Package { get; set; } = string.Empty;
}
