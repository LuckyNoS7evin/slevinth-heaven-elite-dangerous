using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class MissionsEvent : EventBase
{
    [JsonPropertyName("Active")]
    public List<MissionInfo> Active { get; set; } = [];

    [JsonPropertyName("Failed")]
    public List<MissionInfo> Failed { get; set; } = [];

    [JsonPropertyName("Complete")]
    public List<MissionInfo> Complete { get; set; } = [];
}
