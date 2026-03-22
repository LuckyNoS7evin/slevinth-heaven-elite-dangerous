using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class ScanOrganicEvent : EventBase
{
    [JsonPropertyName("ScanType")]
    public string ScanType { get; set; } = string.Empty;

    [JsonPropertyName("Genus")]
    public string Genus { get; set; } = string.Empty;

    [JsonPropertyName("Genus_Localised")]
    public string Genus_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Species")]
    public string Species { get; set; } = string.Empty;

    [JsonPropertyName("Species_Localised")]
    public string Species_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Variant")]
    public string Variant { get; set; } = string.Empty;

    [JsonPropertyName("Variant_Localised")]
    public string Variant_Localised { get; set; } = string.Empty;

    [JsonPropertyName("WasLogged")]
    public bool? WasLogged { get; set; }

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("Body")]
    public int? Body { get; set; }
}
