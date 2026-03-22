using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class MissionInfo
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Extra { get; set; } = [];

    [JsonPropertyName("MissionID")]
    public long? MissionID { get; set; }

    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("LocalisedName")]
    public string LocalisedName { get; set; } = string.Empty;
}
