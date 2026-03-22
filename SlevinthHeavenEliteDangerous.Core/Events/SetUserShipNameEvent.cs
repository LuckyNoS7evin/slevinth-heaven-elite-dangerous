using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class SetUserShipNameEvent : EventBase
{
    [JsonPropertyName("Ship")]
    public string Ship { get; set; } = string.Empty;

    [JsonPropertyName("ShipID")]
    public int? ShipID { get; set; }

    [JsonPropertyName("UserShipName")]
    public string UserShipName { get; set; } = string.Empty;

    [JsonPropertyName("UserShipId")]
    public string UserShipId { get; set; } = string.Empty;
}
