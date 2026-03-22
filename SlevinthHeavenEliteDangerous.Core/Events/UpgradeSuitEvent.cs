using System.Text.Json.Serialization;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class UpgradeSuitEvent : EventBase
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Name_Localised")]
    public string Name_Localised { get; set; } = string.Empty;

    [JsonPropertyName("SuitID")]
    public long? SuitID { get; set; }

    [JsonPropertyName("Class")]
    public int? Class { get; set; }

    [JsonPropertyName("Cost")]
    public long? Cost { get; set; }

    [JsonPropertyName("Resources")]
    public List<ManufacturedItem> Resources { get; set; } = [];
}
