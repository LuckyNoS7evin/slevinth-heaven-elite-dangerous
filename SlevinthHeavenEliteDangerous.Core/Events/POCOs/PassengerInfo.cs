using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class PassengerInfo
{
    [JsonPropertyName("MissionID")]
    public long? MissionID { get; set; }

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("VIP")]
    public bool VIP { get; set; } = false;

    [JsonPropertyName("Wanted")]
    public bool Wanted { get; set; } = false;

    [JsonPropertyName("Count")]
    public int Count { get; set; } = 0;
}
