using System;
using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Pure data model representing the complete ExoBio state (serves as both domain model and persistence type)
/// </summary>
public class ExoBioStateModel
{
    public long SubmittedTotal { get; set; }
    public DateTime? LastEventTime { get; set; }
    public HashSet<string> Keys { get; set; } = [];
    public List<ExoBioDiscoveryModel> Discoveries { get; set; } = [];
    public List<ExoBioSaleModel> Sales { get; set; } = [];
}
