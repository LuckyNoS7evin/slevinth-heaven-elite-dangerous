using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SellSuitEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Price")]
    public long? Price { get; set; }

    [JsonPropertyName("SuitID")]
    public long? SuitID { get; set; }

    [JsonPropertyName("SuitMods")]
    public List<string> SuitMods { get; set; } = [];
}
