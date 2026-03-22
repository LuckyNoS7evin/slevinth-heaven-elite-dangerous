using SlevinthHeavenEliteDangerous.Events;
using System;
using System.Collections.Generic;
using System.IO;

namespace SlevinthHeavenEliteDangerous.Helpers;

/// <summary>
/// Monitors a folder for journal file changes and parses new events using EventParser.
/// Tracks file positions to only process new content as it's added.
/// </summary>
public partial class FileListener : IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly EventParser _parser;
    private readonly Dictionary<string, long> _filePositions;
    private bool _disposed;

    /// <summary>
    /// Raised when an event is successfully parsed from a journal file.
    /// </summary>
    public event Action<EventBase>? EventReceived;

    /// <summary>
    /// Raised when a line cannot be parsed or the event type is unknown.
    /// </summary>
    public event Action<string>? UnknownEventReceived;

    /// <summary>
    /// Raised when an error occurs during file processing or parsing.
    /// </summary>
    public event Action<Exception, string>? ErrorOccurred;

    /// <summary>
    /// Creates a new FileListener to monitor the specified folder.
    /// </summary>
    /// <param name="folderPath">The folder path to monitor for .log files</param>
    /// <param name="parser">The EventParser to use for parsing journal entries</param>
    public FileListener(EventParser parser)
    {
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Saved Games\Frontier Developments\Elite Dangerous";

        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _filePositions = [];

        _watcher = new FileSystemWatcher(folderPath)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            Filter = "*.log",
            EnableRaisingEvents = false,
            InternalBufferSize = 65536  // Increase buffer from 8KB to 64KB to prevent overflow
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileCreated;
        _watcher.Error += OnWatcherError;
    }

    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        var ex = e.GetException();
        System.Diagnostics.Debug.WriteLine($"FileWatcher error: {ex?.Message}");
        ErrorOccurred?.Invoke(ex ?? new Exception("Unknown watcher error"), "FileWatcher error occurred");
    }

    /// <summary>
    /// Starts monitoring the configured folder for file changes.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _watcher.EnableRaisingEvents = true;
    }

    /// <summary>
    /// Stops monitoring the folder for file changes.
    /// </summary>
    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        ProcessFileChanges(e.FullPath);
    }

    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        ProcessNewFile(e.FullPath);
    }

    private void ProcessNewFile(string filePath)
    {
        try
        {
            _parser.ParseFile(
                filePath,
                onEvent: ev => EventReceived?.Invoke(ev),
                onUnknown: msg => UnknownEventReceived?.Invoke(msg),
                onError: (ex, context) => ErrorOccurred?.Invoke(ex, context)
            );

            // Track file size after processing
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                _filePositions[filePath] = fileInfo.Length;
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(ex, $"Failed to process new file: {filePath}");
        }
    }

    private void ProcessFileChanges(string filePath)
    {
        try
        {
            // Reduced delay - just enough for write to complete
            System.Threading.Thread.Sleep(10);

            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                return;

            long lastPosition = 0;
            if (_filePositions.TryGetValue(filePath, out var pos))
            {
                lastPosition = pos;
            }

            // Only process if file has grown
            if (fileInfo.Length <= lastPosition)
                return;

            // Retry logic for file access
            const int maxRetries = 5;
            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    stream.Seek(lastPosition, SeekOrigin.Begin);

                    using var reader = new StreamReader(stream);
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        if (_parser.TryParseLine(line, out var ev, out var eventName, out var error, out var serializationFailure, filePath))
                        {
                            if (ev != null)
                                EventReceived?.Invoke(ev);
                        }
                        else
                        {
                            var prefix = serializationFailure != null ? "Serialization failure" : "Failed to parse";
                            UnknownEventReceived?.Invoke($"{prefix}: event={eventName ?? "<none>"}, reason={error}");
                        }
                    }

                    // Update file position after successful read
                    _filePositions[filePath] = stream.Position;

                    // Break out of retry loop on success
                    break;
                }
                catch (IOException) when (retry < maxRetries - 1)
                {
                    // File might be locked by Elite Dangerous, wait briefly and retry
                    System.Threading.Thread.Sleep(50);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(ex, $"Failed to process file changes: {filePath}");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _watcher?.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}

