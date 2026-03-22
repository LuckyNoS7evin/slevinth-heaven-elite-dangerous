using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CargoTransferEntry
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Count")]
    public int Count { get; set; } = 0;

    [JsonPropertyName("Direction")]
    public string Direction { get; set; } = string.Empty;
}
