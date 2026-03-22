using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class SupercruiseExitEvent : EventBase
{
    [JsonPropertyName("Taxi")]
    public bool? Taxi { get; set; }

    [JsonPropertyName("Multicrew")]
    public bool? Multicrew { get; set; }

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("Body")]
    public string Body { get; set; } = string.Empty;

    [JsonPropertyName("BodyID")]
    public int? BodyID { get; set; }

    [JsonPropertyName("BodyType")]
    public string BodyType { get; set; } = string.Empty;
}
