using System.Text.Json.Serialization;
using System.Collections.Generic;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class StoredShipsEvent : EventBase
{
    [JsonPropertyName("StationName")]
    public string StationName { get; set; } = string.Empty;

    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("StarSystem")]
    public string StarSystem { get; set; } = string.Empty;

    [JsonPropertyName("ShipsHere")]
    public List<StoredShipInfo> ShipsHere { get; set; } = new List<StoredShipInfo>();

    [JsonPropertyName("ShipsRemote")]
    public List<StoredShipInfo> ShipsRemote { get; set; } = new List<StoredShipInfo>();
}
