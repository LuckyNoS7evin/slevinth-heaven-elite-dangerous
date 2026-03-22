using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class DataEntry
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("OwnerID")]
    public long? OwnerID { get; set; }

    [JsonPropertyName("Count")]
    public long? Count { get; set; }
}
