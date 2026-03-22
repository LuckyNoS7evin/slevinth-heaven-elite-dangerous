using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class BackpackChangeEvent : EventBase
{
    [JsonPropertyName("Added")]
    public List<BackpackItem> Added { get; set; } = [];

    [JsonPropertyName("Removed")]
    public List<BackpackItem> Removed { get; set; } = [];
}
