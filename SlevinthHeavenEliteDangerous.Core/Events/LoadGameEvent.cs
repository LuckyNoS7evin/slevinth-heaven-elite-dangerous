using System.Text.Json.Serialization;
using System;
using System.Text.Json;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class LoadGameEvent : EventBase
{
    [JsonPropertyName("FID")]
    public string FID { get; set; } = string.Empty;

    [JsonPropertyName("Commander")]
    public string Commander { get; set; } = string.Empty;

    [JsonPropertyName("Horizons")]
    public bool Horizons { get; set; }

    [JsonPropertyName("Odyssey")]
    public bool Odyssey { get; set; }

    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("Ship_Localised")]
    public string Ship_Localised { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public long ShipID { get; set; }

    [JsonPropertyName("ShipName")]
    public string ShipName { get; set; } = string.Empty;

    [JsonPropertyName("ShipIdent")]
    public string ShipIdent { get; set; } = string.Empty;

    [JsonPropertyName("FuelLevel")]
    public double FuelLevel { get; set; }

    [JsonPropertyName("FuelCapacity")]
    public double FuelCapacity { get; set; }

    [JsonPropertyName("StartLanded")]
    public bool? StartLanded { get; set; }

    [JsonPropertyName("StartDead")]
    public bool? StartDead { get; set; }

    [JsonPropertyName("GameMode")]
    public string GameMode { get; set; } = string.Empty;

    [JsonPropertyName("Credits")]
    public long Credits { get; set; }

    [JsonPropertyName("Loan")]
    public long Loan { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("gameversion")]
    public string GameVersion { get; set; } = string.Empty;

    [JsonPropertyName("build")]
    public string Build { get; set; } = string.Empty;
}
