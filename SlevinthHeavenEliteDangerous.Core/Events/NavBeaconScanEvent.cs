using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events;

public class NavBeaconScanEvent : EventBase
{
    [JsonPropertyName("SystemAddress")]
    public long? SystemAddress { get; set; }

    [JsonPropertyName("NumBodies")]
    public int? NumBodies { get; set; }
}
