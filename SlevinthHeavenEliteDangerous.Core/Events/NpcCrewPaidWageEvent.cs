using System.Text.Json.Serialization;
using System;

namespace SlevinthHeavenEliteDangerous.Events;

public class NpcCrewPaidWageEvent : EventBase
{
    [JsonPropertyName("NpcCrewName")]
    public string NpcCrewName { get; set; } = string.Empty;

    [JsonPropertyName("NpcCrewId")]
    public long NpcCrewId { get; set; }

    [JsonPropertyName("Amount")]
    public long Amount { get; set; }
}
