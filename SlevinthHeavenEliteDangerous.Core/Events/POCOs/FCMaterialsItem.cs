using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class FCMaterialsItem
{
    [JsonPropertyName("id")]
    public long Id { get; set; } = 0;

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Price")]
    public long Price { get; set; } = 0;

    [JsonPropertyName("Stock")]
    public int Stock { get; set; } = 0;

    [JsonPropertyName("Demand")]
    public int Demand { get; set; } = 0;
}
