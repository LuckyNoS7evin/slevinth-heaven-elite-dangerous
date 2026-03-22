using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class MaterialTradeInfo
{
    [JsonPropertyName("Material")]
    public string Material { get; set; } = string.Empty;

    [JsonPropertyName("Material_Localised")]
    public string Material_Localised { get; set; } = string.Empty;

    [JsonPropertyName("Category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("Quantity")]
    public int Quantity { get; set; } = 0;
}
