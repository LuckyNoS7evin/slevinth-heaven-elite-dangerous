namespace SlevinthHeavenEliteDangerous.Core.Models;

/// <summary>
/// Serialisable result of a journal file diagnostic scan.
/// Produced by the desktop app and POSTed to the API.
/// </summary>
public class DiagnosticsReport
{
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Event types found in journal files that have no corresponding C# class.</summary>
    public List<string> MissingEvents { get; set; } = [];

    /// <summary>Sample raw JSON line for each missing event type.</summary>
    public Dictionary<string, string> MissingEventSamples { get; set; } = [];

    /// <summary>JSON properties present in a journal event that are not mapped to the C# model.</summary>
    public Dictionary<string, List<string>> MissingProperties { get; set; } = [];

    /// <summary>Sample raw JSON line for each event that has missing properties.</summary>
    public Dictionary<string, string> MissingPropertySamples { get; set; } = [];

    /// <summary>Events that contain rank-related properties (Combat, Trade, Explore, etc.).</summary>
    public Dictionary<string, List<string>> EventsWithRankProperties { get; set; } = [];

    /// <summary>Journal events that matched a CLR type but failed during JSON deserialization.</summary>
    public List<EventSerializationFailure> SerializationFailures { get; set; } = [];
}
