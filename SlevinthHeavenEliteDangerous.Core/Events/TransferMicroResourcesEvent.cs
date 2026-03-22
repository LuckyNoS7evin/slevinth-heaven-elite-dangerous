using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class TransferMicroResourcesEvent : EventBase
{
    [JsonPropertyName("Transfers")]
    public List<MicroResourceTransferItem> Transfers { get; set; } = [];
}
