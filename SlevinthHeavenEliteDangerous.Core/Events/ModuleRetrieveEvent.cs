using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ModuleRetrieveEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("Slot")]
    public string Slot { get; set; } = string.Empty;

    [JsonPropertyName("RetrievedItem")]
    public string RetrievedItem { get; set; } = string.Empty;

    [JsonPropertyName("RetrievedItem_Localised")]
    public string RetrievedItem_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int ShipID { get; set; }

    [JsonPropertyName("Hot")]
    public bool Hot { get; set; }

    [JsonPropertyName("EngineerModifications")]
    public string EngineerModifications { get; set; } = string.Empty;

    [JsonPropertyName("Level")]
    public int? Level { get; set; }

    [JsonPropertyName("Quality")]
    public double? Quality { get; set; }

    [JsonPropertyName("SwapOutItem")]
    public string SwapOutItem { get; set; } = string.Empty;

    [JsonPropertyName("SwapOutItem_Localised")]
    public string SwapOutItem_Localised { get; set; } = string.Empty;
}
