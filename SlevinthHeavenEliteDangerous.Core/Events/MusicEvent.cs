using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class MusicEvent : EventBase
{
    [JsonPropertyName("MusicTrack")]
    public string MusicTrack { get; set; } = string.Empty;
}
