using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class FactionInfo
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extra { get; set; } = [];

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Faction")]
    public string Faction { get; set; } = string.Empty;

    [JsonPropertyName("FactionState")]
    public string FactionState { get; set; } = string.Empty;

    [JsonPropertyName("Government")]
    public string Government { get; set; } = string.Empty;

    [JsonPropertyName("Allegiance")]
    public string Allegiance { get; set; } = string.Empty;
}
