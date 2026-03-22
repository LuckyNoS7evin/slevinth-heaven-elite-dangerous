using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class CarrierCrewMember
{
    [JsonPropertyName("CrewRole")]
    public string CrewRole { get; set; } = string.Empty;

    [JsonPropertyName("Activated")]
    public bool Activated { get; set; } = false;

    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("CrewName")]
    public string CrewName { get; set; } = string.Empty;
}
