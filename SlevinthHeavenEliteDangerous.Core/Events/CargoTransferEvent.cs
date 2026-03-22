using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class CargoTransferEvent : EventBase
{
    [JsonPropertyName("Transfers")]
    public List<CargoTransferEntry> Transfers { get; set; } = [];
}
