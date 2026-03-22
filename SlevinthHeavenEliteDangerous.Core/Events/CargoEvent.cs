using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class CargoEvent : EventBase
{
    [JsonPropertyName("Vessel")]
    public string Vessel { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int Count { get; set; }

    [JsonPropertyName("Inventory")]
    public List<InventoryEntry> Inventory { get; set; } = [];
}
