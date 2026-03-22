using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CodexEntryEvent : EventBase
{
    [JsonPropertyName("EntryID")]
    public long? EntryID { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SubCategory")]
    public string SubCategory { get; set; } = string.Empty;

    [JsonPropertyName("SubCategory_Localised")]
    public string SubCategory_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("Category_Localised")]
    public string Category_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Region")]
    public string Region { get; set; } = string.Empty;

    [JsonPropertyName("Region_Localised")]
    public string Region_Localised { get; set; } = string.Empty;

    [JsonPropertyName("System")]
    public string System { get; set; } = string.Empty;

    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("BodyID")]
    public int? BodyID { get; set; }

    [JsonPropertyName("NearestDestination")]
    public string NearestDestination { get; set; } = string.Empty;

    [JsonPropertyName("Latitude")]
    public double? Latitude { get; set; }

    [JsonPropertyName("Longitude")]
    public double? Longitude { get; set; }

    [JsonPropertyName("VoucherAmount")]
    public long? VoucherAmount { get; set; }

    [JsonPropertyName("IsNewEntry")]
    public bool? IsNewEntry { get; set; }
}
