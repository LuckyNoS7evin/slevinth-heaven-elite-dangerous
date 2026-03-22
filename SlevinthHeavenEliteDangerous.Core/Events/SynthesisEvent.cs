using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class SynthesisEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Materials")]
    public List<ManufacturedItem> Materials { get; set; } = [];
}
