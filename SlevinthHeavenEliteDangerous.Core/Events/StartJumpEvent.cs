using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class StartJumpEvent : EventBase
{
    [JsonPropertyName("JumpType")]
    public string JumpType { get; set; } = string.Empty;

    [JsonPropertyName("Taxi")]
    public bool? Taxi { get; set; }

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("StarClass")]
    public string StarClass { get; set; } = string.Empty;
}
