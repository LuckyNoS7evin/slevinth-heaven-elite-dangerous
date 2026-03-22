using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Represents a visited star system — pure data, no UI concerns.
/// </summary>
public class VisitedSystemCard
{
    private readonly Dictionary<int, BodyCard> _bodiesDict = [];

    public long SystemAddress { get; set; }
    public string StarSystem { get; set; } = string.Empty;
    public double[]? StarPos { get; set; }
    public double DistanceFromSol { get; set; }
    public DateTime FirstVisitTimestamp { get; set; }
    public DateTime LastVisitTimestamp { get; set; }

    /// <summary>
    /// Flat list of all bodies for JSON serialization — reads from/writes to the internal dictionary.
    /// Uses "FlatBodies" JSON name for backward compatibility with existing save files.
    /// </summary>
    [JsonPropertyName("FlatBodies")]
    public List<BodyCard> Bodies
    {
        get => GetAllBodiesFlat().ToList();
        set
        {
            _bodiesDict.Clear();
            foreach (var body in value)
                _bodiesDict[body.BodyID] = body;
        }
    }

    public BodyCard? GetBodyByID(int bodyID)
        => _bodiesDict.TryGetValue(bodyID, out var body) ? body : null;

    public void RegisterBody(BodyCard body)
        => _bodiesDict[body.BodyID] = body;

    public IEnumerable<BodyCard> GetAllBodiesFlat()
        => _bodiesDict.Values;
}
