using SlevinthHeavenEliteDangerous.Core.Models;
using SlevinthHeavenEliteDangerous.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Helpers;

/// <summary>
/// Diagnostic tool to scan journal files for missing events and properties
/// </summary>
public class EventDiagnostics(EventParser parser)
{
    private readonly EventParser _parser = parser ?? throw new ArgumentNullException(nameof(parser));
    private readonly HashSet<string> _missingEvents = [];
    private readonly Dictionary<string, HashSet<string>> _missingProperties = [];

    // Rank properties from PromotionEvent
    private static readonly HashSet<string> _rankProperties = 
    [
        "Combat", "Trade", "Explore", "Soldier", "Exobiologist", 
        "Empire", "Federation", "CQC"
    ];

    /// <summary>
    /// Scan all journal files in the directory for missing events and properties
    /// </summary>
    public DiagnosticsReport ScanAllFiles(string folderPath)
    {
        var result = new DiagnosticResult();
        
        try
        {
            var logFiles = Directory.GetFiles(folderPath, "*.log")
                .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"Scanning {logFiles.Count} journal files for missing events and properties...");

            foreach (var filePath in logFiles)
            {
                ScanFile(filePath, result);
            }

            System.Diagnostics.Debug.WriteLine($"Scan complete: {result.MissingEvents.Count} missing events, {result.MissingProperties.Count} events with missing properties");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scanning files: {ex.Message}");
        }

        return result.ToReport();
    }

    private void ScanFile(string filePath, DiagnosticResult result)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                AnalyzeLine(line, result);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error scanning file {filePath}: {ex.Message}");
        }
    }

    private void AnalyzeLine(string line, DiagnosticResult result)
    {
        try
        {
            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            if (!root.TryGetProperty("event", out var eventProperty))
                return;

            string eventName = eventProperty.GetString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(eventName))
                return;

            // Try to parse the event
            if (_parser.TryParseLine(line, out var parsedEvent, out _, out _, out var serializationFailure))
            {
                if (parsedEvent != null)
                {
                    // Event exists, check for missing properties
                    CheckMissingProperties(root, parsedEvent, eventName, line, result);
                }
                else
                {
                    // Event type not implemented
                    result.MissingEvents.Add(eventName);
                    result.MissingEventSamples[eventName] = line;
                }
            }
            else
            {
                if (serializationFailure != null)
                {
                    result.AddSerializationFailure(serializationFailure);
                }
                else
                {
                    // Failed to parse - event might not exist
                    result.MissingEvents.Add(eventName);
                    result.MissingEventSamples[eventName] = line;
                }
            }
        }
        catch (JsonException)
        {
            // Invalid JSON, skip
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error analyzing line: {ex.Message}");
        }
    }

    private static void CheckMissingProperties(JsonElement root, EventBase parsedEvent, string eventName, string rawLine, DiagnosticResult result)
    {
        var eventType = parsedEvent.GetType();
        var eventProperties = eventType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .SelectMany(p =>
            {
                var jsonAttr = p.GetCustomAttribute<JsonPropertyNameAttribute>();
                return jsonAttr != null
                    ? new[] { p.Name.ToLowerInvariant(), jsonAttr.Name.ToLowerInvariant() }
                    : new[] { p.Name.ToLowerInvariant() };
            })
            .ToHashSet();

        var missingProps = new HashSet<string>();
        var foundRankProps = new HashSet<string>();

        foreach (var jsonProperty in root.EnumerateObject())
        {
            string propName = jsonProperty.Name;

            // Skip standard properties that all events have
            if (propName is "timestamp" or "event")
                continue;

            // Check if this property is a Rank property
            if (_rankProperties.Contains(propName))
            {
                foundRankProps.Add(propName);
            }

            // Check if property exists in the event class (case-insensitive)
            if (!eventProperties.Contains(propName.ToLowerInvariant()))
            {
                missingProps.Add(propName);
            }
        }

        // Track events that mention Rank properties
        if (foundRankProps.Count > 0)
        {
            if (!result.EventsWithRankProperties.ContainsKey(eventName))
            {
                result.EventsWithRankProperties[eventName] = [];
            }

            foreach (var rankProp in foundRankProps)
            {
                result.EventsWithRankProperties[eventName].Add(rankProp);
            }
        }

        if (missingProps.Count > 0)
        {
            if (!result.MissingProperties.ContainsKey(eventName))
            {
                result.MissingProperties[eventName] = [];
                result.MissingPropertySamples[eventName] = rawLine;
            }

            foreach (var prop in missingProps)
            {
                result.MissingProperties[eventName].Add(prop);
            }
        }
    }
}

/// <summary>
/// Internal working state used during a diagnostic scan.
/// Uses HashSet collections for efficient deduplication.
/// Call <see cref="ToReport"/> to get a serialisable <see cref="DiagnosticsReport"/>.
/// </summary>
internal class DiagnosticResult
{
    public HashSet<string> MissingEvents { get; } = [];
    public Dictionary<string, string> MissingEventSamples { get; } = [];
    public Dictionary<string, HashSet<string>> MissingProperties { get; } = [];
    public Dictionary<string, string> MissingPropertySamples { get; } = [];
    public Dictionary<string, HashSet<string>> EventsWithRankProperties { get; } = [];
    public Dictionary<string, EventSerializationFailure> SerializationFailures { get; } = [];

    public void AddSerializationFailure(EventSerializationFailure failure)
    {
        var key = $"{failure.EventName}|{failure.ClrTypeName}|{failure.ExceptionType}|{failure.Error}";
        SerializationFailures.TryAdd(key, failure);
    }

    public DiagnosticsReport ToReport() => new()
    {
        GeneratedAt = DateTime.UtcNow,
        MissingEvents = [.. MissingEvents],
        MissingEventSamples = new(MissingEventSamples),
        MissingProperties = MissingProperties.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.OrderBy(p => p).ToList()),
        MissingPropertySamples = new(MissingPropertySamples),
        EventsWithRankProperties = EventsWithRankProperties.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.OrderBy(p => p).ToList()),
        SerializationFailures = [.. SerializationFailures.Values.OrderBy(f => f.EventName).ThenBy(f => f.Error)]
    };
}
