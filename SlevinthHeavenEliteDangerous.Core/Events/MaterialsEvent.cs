using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using SlevinthHeavenEliteDangerous.Events.POCOs;

namespace SlevinthHeavenEliteDangerous.Events;

public class MaterialsEvent : EventBase
{
    [JsonPropertyName("Raw")]
    public List<RawMaterial> Raw { get; set; } = [];

    [JsonPropertyName("Manufactured")]
    public List<ManufacturedItem> Manufactured { get; set; } = [];

    [JsonPropertyName("Encoded")]
    public List<EncodedItem> Encoded { get; set; } = [];
}
