using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SupercruiseEntryEvent : EventBase
{
    [JsonPropertyName("Taxi")]
    public bool? Taxi { get; set; }

    [JsonPropertyName("Multicrew")]
    public bool? Multicrew { get; set; }

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }
}
