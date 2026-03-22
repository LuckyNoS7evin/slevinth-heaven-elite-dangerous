using SlevinthHeavenEliteDangerous.Events.POCOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SAASignalsFoundEvent : EventBase
{
    [JsonPropertyName("BodyName")]
    public string BodyName { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("BodyID")]
    public int? BodyID { get; set; }

    [JsonPropertyName("Signals")]
    public List<SignalEntry> Signals { get; set; } = [];

    [JsonPropertyName("Genuses")]
    public List<GenusEntry> Genuses { get; set; } = [];
}
