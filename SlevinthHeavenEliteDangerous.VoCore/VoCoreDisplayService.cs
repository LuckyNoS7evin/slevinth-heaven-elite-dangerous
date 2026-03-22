using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.VoCore.Renderers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace SlevinthHeavenEliteDangerous.VoCore;

/// <summary>
/// Listens to Elite Dangerous journal events and drives the VoCore Screen display.
/// Rotates between the current system info screen and ExoBiology progress every 10 seconds.
/// Registered as an IEventHandler so it receives events via JournalEventService automatically.
/// </summary>
public sealed class VoCoreDisplayService :  IDisposable
{
    private UsbDisplayWriter? _writer;
    private readonly Timer    _timer;
    public bool Enabled { get; private set; }
    private readonly object           _lock    = new();

    // --- Display state ---
    private string               _currentSystem = string.Empty;
    private double               _distanceFromSol;
    private double               _lastJumpDist;
    private ActiveDiscovery?     _lastDiscovery;

    // --- Rotation ---
    private bool _showExoBio;
    private readonly List<(string Name, string Reason, double Distance)> _valuableBodies = new();

    public VoCoreDisplayService()
    {
        // Do not open USB device until the service is explicitly enabled.
        _writer = null;
        // Create timer but do not start it until service is enabled.
        _timer = new Timer(OnRotationTick, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        Enabled = false;
    }


    // Lightweight handlers called by data services via StartupService.
    // We store only the most recent discovery and display its full details.
    public void HandleDiscoveryAdded(string key, string title, string details, string scanType, long sampleValue, long estimatedValue, long estimatedBonus, string systemName, string bodyName, double distanceFromSol)
    {
        lock (_lock)
        {
            _lastDiscovery = new ActiveDiscovery
            {
                Key = key ?? string.Empty,
                Name = title ?? string.Empty,
                Species = string.Empty,
                Details = details ?? string.Empty,
                ScanType = scanType ?? string.Empty,
                SampleValue = sampleValue,
                EstimatedValue = estimatedValue,
                EstimatedBonus = estimatedBonus,
                SystemName = systemName ?? string.Empty,
                BodyName = bodyName ?? string.Empty,
                DistanceFromSol = distanceFromSol
            };
            // Switch to ExoBio screen for new discovery
            _showExoBio = true;
        }

        if (!Enabled) return;
        Render();
    }

    public void HandleDiscoveryUpdated(string key, string scanType, long estimatedValue, long estimatedBonus)
    {
        lock (_lock)
        {
            if (_lastDiscovery != null && _lastDiscovery.Key == key)
            {
                _lastDiscovery = _lastDiscovery with { ScanType = scanType ?? _lastDiscovery.ScanType, EstimatedValue = estimatedValue, EstimatedBonus = estimatedBonus };
            }
            // Switch to ExoBio screen for discovery updates
            _showExoBio = true;
        }
        if (!Enabled) return;
        Render();
    }

    public void HandleDataLoaded(IEnumerable<(string Key, string Title, string Details, string ScanType, long SampleValue, long EstimatedValue, long EstimatedBonus, string SystemName, string BodyName, double DistanceFromSol)> discoveries)
    {
        lock (_lock)
        {
            var last = discoveries.LastOrDefault();
            if (!string.IsNullOrEmpty(last.Key))
            {
                _lastDiscovery = new ActiveDiscovery
                {
                    Key = last.Key,
                    Name = last.Title,
                    Details = last.Details,
                    ScanType = last.ScanType,
                    SampleValue = last.SampleValue,
                    EstimatedValue = last.EstimatedValue,
                    EstimatedBonus = last.EstimatedBonus,
                    SystemName = last.SystemName,
                    BodyName = last.BodyName,
                    DistanceFromSol = last.DistanceFromSol
                };
                // If data loaded contains discoveries, make ExoBio the active screen
                _showExoBio = true;
            }
        }
        if (!Enabled) return;
        Render();
    }

    public void HandleDiscoveriesSubmitted()
    {
        lock (_lock)
        {
            _lastDiscovery = null;
            _showExoBio = false; // return to system view after submit
        }
        if (Enabled) Render();
    }

   
    // -------------------------------------------------------------------------
    // Rotation timer
    // -------------------------------------------------------------------------

    private void OnRotationTick(object? _)
    {
        // Rotation disabled: do not auto-toggle screens. This handler remains to keep the
        // timer object available but it performs no action.
        return;
    }

    // -------------------------------------------------------------------------
    // Rendering
    // -------------------------------------------------------------------------

    private void Render()
    {
        if (!Enabled) return;
        string             system;
        double             dist, jumpDist;
        ActiveDiscovery?   lastDiscovery;
        bool               showExoBio = _showExoBio;

        lock (_lock)
        {
            system      = _currentSystem;
            dist        = _distanceFromSol;
            jumpDist    = _lastJumpDist;
            lastDiscovery = _lastDiscovery;
        }

        try
        {
            if (_writer == null) return;
            int w = _writer.Width;
            int h = _writer.Height;

            byte[] frame = showExoBio
                ? (lastDiscovery != null ? ExoBioScreen.RenderLastDiscovery(lastDiscovery, w, h) : ExoBioScreen.Render(Array.Empty<ActiveDiscovery>(), w, h))
                : SystemInfoScreen.Render(system, dist, jumpDist, _valuableBodies, w, h);

            _writer.WriteFrame(frame);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VoCore] Render error: {ex.Message}");
        }
    }

    // -------------------------------------------------------------------------

    public void Dispose()
    {
        _timer.Dispose();
        try { _writer?.Dispose(); } catch { }
    }

    // -------------------------------------------------------------------------
    // Handlers driven by data services via StartupService
    // -------------------------------------------------------------------------

    public void HandleSystemUpdate(string systemName, double distanceFromSol)
    {
        lock (_lock)
        {
            _currentSystem = systemName ?? string.Empty;
            _distanceFromSol = distanceFromSol;
        }

        _showExoBio = false;
        if (Enabled) Render();
    }
    public void HandleValuableBodyAdded(string bodyName, string reason, double distance)
    {
        if (string.IsNullOrWhiteSpace(bodyName)) return;
        lock (_lock)
        {
            if (!_valuableBodies.Any(v => v.Name == bodyName))
                _valuableBodies.Add((bodyName, reason ?? string.Empty, distance));
            // Show system screen when valuable body info arrives
            _showExoBio = false;
        }
        if (Enabled) Render();
    }

    public void HandleValuableBodiesSnapshot(IEnumerable<(string Name, string Reason, double Distance)> bodies)
    {
        lock (_lock)
        {
            _valuableBodies.Clear();
            if (bodies != null)
                _valuableBodies.AddRange(bodies.Where(b => !string.IsNullOrWhiteSpace(b.Name)));
            // Snapshot should result in system view
            _showExoBio = false;
        }
        if (Enabled) Render();
    }

    public void HandleValuableBodiesCleared()
    {
        lock (_lock) _valuableBodies.Clear();
        if (Enabled) Render();
    }

    /// <summary>
    /// Enable the VoCore service — begins reacting to journal events and rendering frames.
    /// </summary>
    public void StartService()
    {
        if (Enabled) return;
        Enabled = true;
        if (_writer == null)
            _writer = new UsbDisplayWriter(VoCoreSettings.Load());

        // Default to Current System view when enabling
        lock (_lock)
        {
            _showExoBio = false;
        }
        Render();
    }

    /// <summary>
    /// Disable the VoCore service — stop handling events and turn the display off.
    /// </summary>
    public void StopService()
    {
        if (!Enabled) return;
        Enabled = false;
        try
        {
            // Best-effort: send display-off by disposing the writer
            _writer?.Dispose();
            // Stop rotation timer
            try { _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan); } catch { }
        }
        catch { }
        _writer = null;
    }

    /// <summary>
    /// Query whether the VoCore USB device appears present.
    /// </summary>
    public bool IsDevicePresent() => UsbDeviceDetector.IsDevicePresent();
}

/// <summary>Discovery tracked by VoCoreDisplayService for display purposes only.</summary>
public sealed record ActiveDiscovery
{
    public string Key      { get; init; } = string.Empty;
    public string Name     { get; init; } = string.Empty;
    public string Species  { get; init; } = string.Empty;
    public string ScanType { get; init; } = string.Empty;

    // Additional fields for full discovery display
    public string Details { get; init; } = string.Empty;
    public long SampleValue { get; init; }
    public long EstimatedValue { get; init; }
    public long EstimatedBonus { get; init; }
    public string SystemName { get; init; } = string.Empty;
    public string BodyName { get; init; } = string.Empty;
    public double DistanceFromSol { get; init; }

    /// <summary>1 = Log, 2 = Sample, 3 = Analyse</summary>
    public int ScanProgress => ScanType.ToLowerInvariant() switch
    {
        "log"     => 1,
        "sample"  => 2,
        "analyse" => 3,
        _         => 0,
    };
}
