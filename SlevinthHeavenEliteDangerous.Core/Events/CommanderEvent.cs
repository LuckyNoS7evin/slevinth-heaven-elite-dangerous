using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class CommanderEvent : EventBase
{
    [JsonPropertyName("FID")]
    public string FID { get; set; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;
}
