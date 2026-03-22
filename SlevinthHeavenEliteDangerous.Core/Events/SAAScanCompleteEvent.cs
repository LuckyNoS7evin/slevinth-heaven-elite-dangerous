using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SAAScanCompleteEvent : EventBase
{
    [JsonPropertyName("BodyName")]
    public string BodyName { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("BodyID")]
    public int? BodyID { get; set; }

    [JsonPropertyName("ProbesUsed")]
    public int? ProbesUsed { get; set; }

    [JsonPropertyName("EfficiencyTarget")]
    public int? EfficiencyTarget { get; set; }
}
