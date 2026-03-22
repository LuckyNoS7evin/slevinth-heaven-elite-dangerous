using System.Text.Json.Serialization;
using System.Collections.Generic;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class SellOrganicDataEvent : EventBase
{
    [JsonPropertyName("MarketID")]
    public long MarketID { get; set; }

    [JsonPropertyName("BioData")]
    public List<BioDataEntry> BioData { get; set; } = new List<BioDataEntry>();
}
