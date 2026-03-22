using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class EventBase
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;
}
// Individual event types are declared in separate files with explicit
// properties. Keep EventBase here so all event classes can inherit from it.
