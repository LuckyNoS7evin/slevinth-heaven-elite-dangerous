using SlevinthHeavenEliteDangerous.Events.POCOs;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class FSSBodySignalsEvent : EventBase
{
    [JsonPropertyName("BodyName")]
    public string BodyName { get; set; } = string.Empty;

    [JsonPropertyName("BodyID")]
    public int? BodyID { get; set; }

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("Signals")]
    public List<SignalEntry> Signals { get; set; } = [];
}
