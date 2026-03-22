using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class PassengersEvent : EventBase
{
    [JsonPropertyName("Manifest")]
    public List<PassengerInfo> Manifest { get; set; } = [];
}
