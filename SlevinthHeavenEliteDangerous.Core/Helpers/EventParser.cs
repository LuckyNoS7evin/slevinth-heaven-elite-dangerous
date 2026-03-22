using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Helpers;

/// <summary>
/// Discovers Event types (subclasses of <see cref="EventBase"/>) in the current assembly
/// and provides helpers to deserialize a single JSON journal line into the correct event
/// type based on the line's "event" property.
/// If an event type is not found for a given "event" value the parser will call the
/// provided unknown handler or write a trace line. Deserialization failures for known
/// event types are surfaced via <see cref="SerializationFailed"/>.
/// </summary>
public class EventParser
{
private const int MaxRawJsonLength = 4000;
private readonly Dictionary<string, Type> _map;
private readonly JsonSerializerOptions _options;

public event Action<EventSerializationFailure>? SerializationFailed;

public EventParser()
{
    _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    _map = BuildEventMap();
}

    // Build mapping from journal "event" name -> CLR Type
    private static Dictionary<string, Type> BuildEventMap()
    {
        var asm = Assembly.GetExecutingAssembly();
        var types = asm.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(EventBase).IsAssignableFrom(t));

        var dict = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        foreach (var t in types)
        {
            var name = t.Name;
            if (name.EndsWith("Event", StringComparison.OrdinalIgnoreCase))
            {
                name = name[..^"Event".Length];
            }

            // if duplicate, prefer existing (first discovered)
            if (!dict.ContainsKey(name))
                dict[name] = t;
        }

        return dict;
    }

    /// <summary>
    /// Try parse a single JSON line into a specific EventBase-derived instance.
    /// </summary>
    public bool TryParseLine(
        string jsonLine,
        out EventBase? ev,
        out string? eventName,
        out string? error,
        out EventSerializationFailure? serializationFailure,
        string? sourceContext = null)
    {
        ev = null;
        eventName = null;
        error = null;
        serializationFailure = null;

        if (string.IsNullOrWhiteSpace(jsonLine))
        {
            error = "line is empty";
            return false;
        }

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(jsonLine);
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }

        using (doc)
        {
            if (!doc.RootElement.TryGetProperty("event", out var eProp))
            {
                error = "no 'event' property";
                return false;
            }

            eventName = eProp.GetString();
            if (string.IsNullOrWhiteSpace(eventName))
            {
                error = "empty event name";
                return false;
            }

            if (!_map.TryGetValue(eventName, out var type))
            {
                error = $"no CLR type registered for event '{eventName}'";
                return false;
            }

            try
            {
                var obj = JsonSerializer.Deserialize(jsonLine, type, _options);
                ev = obj as EventBase;
                if (ev == null)
                {
                    error = $"deserialized object was not an EventBase for event '{eventName}'";
                    serializationFailure = CreateSerializationFailure(
                        eventName,
                        type,
                        error,
                        sourceContext,
                        jsonLine);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                serializationFailure = CreateSerializationFailure(
                    eventName,
                    type,
                    ex.Message,
                    sourceContext,
                    jsonLine,
                    ex);
                return false;
            }
        }
    }

    /// <summary>
    /// Parse a journal file (one JSON object per line). For each parsed event the
    /// <paramref name="onEvent"/> callback is invoked. If an event cannot be matched
    /// a log entry is written via <paramref name="onUnknown"/> (or Trace if null).
    /// </summary>
    public void ParseFile(string path, Action<EventBase> onEvent, Action<string>? onUnknown = null, Action<Exception,string>? onError = null)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(onEvent);

        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);
        string? line;
        long lineno = 0;
        while ((line = reader.ReadLine()) != null)
        {
            lineno++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                if (TryParseLine(line, out var ev, out var eventName, out var error, out var serializationFailure, $"{path}:{lineno}"))
                {
                    if(ev != null)
                        onEvent(ev);
                }
                else
                {
                    var failurePrefix = serializationFailure != null ? "Serialization failure" : "Unmapped/Failed";
                    var msg = $"{failurePrefix} line {lineno}: event={eventName ?? "<none>"}, reason={error}";
                    if (onUnknown != null)
                        onUnknown(msg);
                    else
                        System.Diagnostics.Trace.WriteLine(msg);
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex, $"line {lineno}");
            }
        }
    }

    /// <summary>
    /// Allow manual registration or overriding mapping from event name to type.
    /// </summary>
    public void Register(string eventName, Type eventType)
    {
        if (string.IsNullOrWhiteSpace(eventName)) throw new ArgumentNullException(nameof(eventName));
        ArgumentNullException.ThrowIfNull(eventType);
        if (!typeof(EventBase).IsAssignableFrom(eventType)) throw new ArgumentException("eventType must derive from EventBase", nameof(eventType));

        _map[eventName] = eventType;
    }

    public void Register<TEvent>(string eventName) where TEvent : EventBase
    {
        Register(eventName, typeof(TEvent));
    }

    private EventSerializationFailure CreateSerializationFailure(
        string eventName,
        Type type,
        string error,
        string? sourceContext,
        string rawJson,
        Exception? exception = null)
    {
        var failure = new EventSerializationFailure
        {
            EventName = eventName,
            ClrTypeName = type.FullName,
            Error = error,
            ExceptionType = exception?.GetType().FullName,
            SourceContext = sourceContext,
            RawJson = rawJson.Length <= MaxRawJsonLength ? rawJson : rawJson[..MaxRawJsonLength] + "..."
        };

        SerializationFailed?.Invoke(failure);
        return failure;
    }
}
