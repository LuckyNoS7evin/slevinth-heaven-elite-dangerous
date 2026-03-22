using System.Text.Json.Serialization;
using System.Collections.Generic;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class ShipLockerEvent : EventBase
{
    [JsonPropertyName("Items")]
    public List<InventoryEntry> Items { get; set; } = [];

    [JsonPropertyName("Components")]
    public List<InventoryEntry> Components { get; set; } = [];

    [JsonPropertyName("Consumables")]
    public List<ConsumableEntry> Consumables { get; set; } = [];

    [JsonPropertyName("Data")]
    public List<DataEntry> Data { get; set; } = [];
}
