using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class PassengerStats
{
    [JsonPropertyName("Passengers_Missions_Accepted")]
    public int? MissionsAccepted { get; set; }

    [JsonPropertyName("Passengers_Missions_Disgruntled")]
    public int? MissionsDisgruntled { get; set; }

    [JsonPropertyName("Passengers_Missions_Bulk")]
    public int? MissionsBulk { get; set; }

    [JsonPropertyName("Passengers_Missions_VIP")]
    public int? MissionsVIP { get; set; }

    [JsonPropertyName("Passengers_Missions_Delivered")]
    public int? MissionsDelivered { get; set; }

    [JsonPropertyName("Passengers_Missions_Ejected")]
    public int? MissionsEjected { get; set; }
}
