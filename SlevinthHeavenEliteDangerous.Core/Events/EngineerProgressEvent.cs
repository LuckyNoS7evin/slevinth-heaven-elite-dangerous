using System.Text.Json.Serialization;
using System.Collections.Generic;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class EngineerProgressEvent : EventBase
{
    [JsonPropertyName("Engineers")]
    public List<EngineerInfo> Engineers { get; set; } = [];

    [JsonPropertyName("Engineer")]
    public string Engineer { get; set; } = string.Empty;

    [JsonPropertyName("EngineerID")]
    public int? EngineerID { get; set; }

    [JsonPropertyName("Progress")]
    public string Progress { get; set; } = string.Empty;

    [JsonPropertyName("Rank")]
    public int? Rank { get; set; }
}

public class EngineerInfo
{
    [JsonPropertyName("Engineer")]
    public string Engineer { get; set; } = string.Empty;

    [JsonPropertyName("EngineerID")]
    public int EngineerID { get; set; }

    [JsonPropertyName("Progress")]
    public string Progress { get; set; } = string.Empty;

    [JsonPropertyName("Rank")]
    public int? Rank { get; set; }

    [JsonPropertyName("RankProgress")]
    public int? RankProgress { get; set; }
}
