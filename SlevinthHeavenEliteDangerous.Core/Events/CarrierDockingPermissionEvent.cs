using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class CarrierDockingPermissionEvent : EventBase
{
    [JsonPropertyName("CarrierID")]
    public long CarrierID { get; set; } = 0;

    [JsonPropertyName("DockingAccess")]
    public string DockingAccess { get; set; } = string.Empty;

    [JsonPropertyName("AllowNotorious")]
    public bool? AllowNotorious { get; set; }
}
