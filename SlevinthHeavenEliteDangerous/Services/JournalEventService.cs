using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Helpers;
using SlevinthHeavenEliteDangerous.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Service for managing journal file monitoring and event routing
/// </summary>
public sealed class JournalEventService : IDisposable
{
    private readonly EventParser _eventParser;
    private readonly FileListener _fileListener;
    private readonly ISlevinthHeavenApi _api;
    private readonly List<IEventHandler> _eventHandlers = [];
    private readonly HashSet<string> _reportedSerializationFailures = [];
    private readonly object _serializationFailureLock = new();
    private bool _isStarted = false;

    public JournalEventService(ISlevinthHeavenApi api)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _eventParser = new EventParser();
        _fileListener = new FileListener(_eventParser);
        _eventParser.SerializationFailed += OnSerializationFailed;

        // Subscribe to FileListener events
        _fileListener.EventReceived += OnEventReceived;
        _fileListener.UnknownEventReceived += OnUnknownEventReceived;
        _fileListener.ErrorOccurred += OnErrorOccurred;
    }

    /// <summary>
    /// Event raised when a game event is received and processed
    /// </summary>
    public event EventHandler<GameEventReceivedEventArgs>? GameEventReceived;

    /// <summary>
    /// Event raised when an unknown event is received
    /// </summary>
    public event EventHandler<UnknownEventReceivedEventArgs>? UnknownEventReceived;

    /// <summary>
    /// Event raised when an error occurs
    /// </summary>
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    /// <summary>
    /// Register an event handler to receive game events
    /// </summary>
    public void RegisterEventHandler(IEventHandler handler)
    {
        if (!_eventHandlers.Contains(handler))
        {
            _eventHandlers.Add(handler);
            System.Diagnostics.Debug.WriteLine($"Registered event handler: {handler.GetType().Name}");
        }
    }

    /// <summary>
    /// Unregister an event handler
    /// </summary>
    public void UnregisterEventHandler(IEventHandler handler)
    {
        _eventHandlers.Remove(handler);
    }

    /// <summary>
    /// Start monitoring journal files
    /// </summary>
    public void Start()
    {
        if (_isStarted)
        {
            return;
        }

        try
        {
            _fileListener.Start();
            _isStarted = true;
            System.Diagnostics.Debug.WriteLine("JournalEventService started monitoring");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to start JournalEventService: {ex.Message}");
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(ex, "Failed to start FileListener"));
            throw;
        }
    }

    /// <summary>
    /// Stop monitoring journal files
    /// </summary>
    public void Stop()
    {
        if (!_isStarted)
        {
            return;
        }

        try
        {
            _fileListener.Stop();
            _isStarted = false;
            System.Diagnostics.Debug.WriteLine("JournalEventService stopped monitoring");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping JournalEventService: {ex.Message}");
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(ex, "Error stopping FileListener"));
        }
    }

    /// <summary>
    /// Run diagnostics on all journal files and POST the result to the API.
    /// </summary>
    public async Task RunDiagnosticsAsync()
    {
        try
        {
            var report = await Task.Run(() =>
            {
                var eventParser = new EventParser();
                var journalPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                    + @"\Saved Games\Frontier Developments\Elite Dangerous";
                var diagnostics = new EventDiagnostics(eventParser);
                return diagnostics.ScanAllFiles(journalPath);
            });

            await _api.PostDiagnosticsAsync(report);
            System.Diagnostics.Debug.WriteLine("[Diagnostics] Report posted to API successfully.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Diagnostics] Error: {ex.Message}");
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(ex, "Error running diagnostics"));
        }
    }

    /// <summary>
    /// Process a single line (for initial data loading)
    /// </summary>
    /// <param name="line">The JSON line to process</param>
    /// <returns>True if the line was successfully parsed and processed</returns>
    public bool ProcessLine(string line, string? sourceContext = null)
    {
        if (_eventParser.TryParseLine(line, out var evt, out _, out _, out _, sourceContext))
        {
            if (evt != null)
            {
                OnEventReceived(evt);
                return true;
            }
        }
        return false;
    }

    public void Dispose()
    {
        Stop();
        _eventParser.SerializationFailed -= OnSerializationFailed;
        _fileListener?.Dispose();
        System.Diagnostics.Debug.WriteLine("JournalEventService disposed");
    }

    private void OnEventReceived(EventBase evt)
    {
       // System.Diagnostics.Debug.WriteLine($"Event received: {evt.Event} at {evt.Timestamp}");

        // Forward event to all registered event handlers
        foreach (var handler in _eventHandlers)
        {
            try
            {
                handler.HandleEvent(evt);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in event handler {handler.GetType().Name}: {ex.Message}");
                ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(ex, $"Error in {handler.GetType().Name}.HandleEvent"));
            }
        }

        // Raise event for UI or other subscribers
        GameEventReceived?.Invoke(this, new GameEventReceivedEventArgs(evt));
    }

    private void OnUnknownEventReceived(string message)
    {
        System.Diagnostics.Debug.WriteLine($"Unknown event: {message}");
        UnknownEventReceived?.Invoke(this, new UnknownEventReceivedEventArgs(message));
    }

    private void OnErrorOccurred(Exception ex, string context)
    {
        System.Diagnostics.Debug.WriteLine($"Error in {context}: {ex.Message}");
        ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(ex, context));
    }

    private void OnSerializationFailed(EventSerializationFailure failure)
    {
        var key = $"{failure.EventName}|{failure.ClrTypeName}|{failure.ExceptionType}|{failure.Error}";

        lock (_serializationFailureLock)
        {
            if (!_reportedSerializationFailures.Add(key))
            {
                return;
            }
        }

        _ = ReportSerializationFailureAsync(failure);
    }

    private async Task ReportSerializationFailureAsync(EventSerializationFailure failure)
    {
        try
        {
            await _api.PostDiagnosticsAsync(new DiagnosticsReport
            {
                GeneratedAt = failure.OccurredAt,
                SerializationFailures = [failure]
            });

            System.Diagnostics.Debug.WriteLine($"[Diagnostics] Reported serialization failure for event '{failure.EventName}'.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Diagnostics] Failed to report serialization failure: {ex.Message}");
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs(ex, "Error reporting serialization failure"));
        }
    }
}

/// <summary>
/// Event args for game event received
/// </summary>
public class GameEventReceivedEventArgs : EventArgs
{
    public EventBase Event { get; }

    public GameEventReceivedEventArgs(EventBase evt)
    {
        Event = evt;
    }
}

/// <summary>
/// Event args for unknown event received
/// </summary>
public class UnknownEventReceivedEventArgs : EventArgs
{
    public string Message { get; }

    public UnknownEventReceivedEventArgs(string message)
    {
        Message = message;
    }
}

/// <summary>
/// Event args for error occurred
/// </summary>
public class ErrorOccurredEventArgs : EventArgs
{
    public Exception Exception { get; }
    public string Context { get; }

    public ErrorOccurredEventArgs(Exception exception, string context)
    {
        Exception = exception;
        Context = context;
    }
}
