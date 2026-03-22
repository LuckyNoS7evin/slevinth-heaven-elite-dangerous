using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class FuelScoopEvent : EventBase
{
    [JsonPropertyName("Scooped")]
    public double Scooped { get; set; }

    [JsonPropertyName("Total")]
    public double Total { get; set; }
}
