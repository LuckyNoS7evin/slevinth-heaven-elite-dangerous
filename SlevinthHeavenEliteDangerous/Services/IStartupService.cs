using System;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Interface for application startup operations
/// </summary>
public interface IStartupService
{
    /// <summary>
    /// Event raised when initialization progress updates
    /// </summary>
    event EventHandler<InitializationProgressEventArgs>? InitializationProgress;

    /// <summary>
    /// Attempts to silently restore a saved Frontier auth session.
    /// Must be called before InitializeDataAsync.
    /// </summary>
    Task TryRestoreAuthAsync();

    /// <summary>
    /// Check if first run and initialize data by scanning journal files if needed
    /// </summary>
    /// <returns>True if initial scan was performed, false if save data already existed</returns>
    Task<bool> InitializeDataAsync();

    /// <summary>
    /// Register all event handlers with the journal event service
    /// </summary>
    void RegisterEventHandlers();

    /// <summary>
    /// Run diagnostics on journal files
    /// </summary>
    void RunDiagnostics();

    /// <summary>
    /// Start the journal event service monitoring
    /// </summary>
    void StartJournalMonitoring();

    /// <summary>
    /// Start the background journal upload service
    /// </summary>
    Task StartJournalUploadAsync();

    /// <summary>
    /// Stop the journal event service monitoring
    /// </summary>
    void StopJournalMonitoring();

    /// <summary>
    /// Stop the background journal upload service
    /// </summary>
    void StopJournalUpload();
}
