using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Watches for valuable body scans and exobio discoveries, maintains the overlay log,
/// and handles loading and saving the log to disk.
/// </summary>
public sealed class OverlayLogService : IEventHandler
{
    private readonly OverlayDataService _dataService = new();
    private readonly List<OverlayLogEntryRecord> _entries = [];
    private const int MaxEntries = 20;
    private DateTime _lastEventTimestamp = DateTime.MinValue;

    private DateTime _lastSaveRequest = DateTime.MinValue;
    private readonly TimeSpan _saveDebounce = TimeSpan.FromMilliseconds(500);
    private Task? _saveTask;
    private readonly object _saveLock = new();
    private bool _isBulkLoading = false;

    // (no cross-event dictionaries needed — FSSBodySignalsEvent carries BodyName directly)

    /// <summary>Raised when persisted log entries have been loaded from disk.</summary>
    public event EventHandler<OverlayLogDataLoadedEventArgs>? DataLoaded;

    /// <summary>Raised when a new entry is added to the log.</summary>
    public event EventHandler<OverlayLogEntryEventArgs>? EntryAdded;


    public async Task LoadAsync()
    {
        var loaded = await _dataService.LoadDataAsync();
        if (loaded != null && loaded.Count > 0)
        {
            _entries.Clear();
            _entries.AddRange(loaded);
            _lastEventTimestamp = loaded
                .Where(e => e.Time.HasValue)
                .Select(e => e.Time!.Value)
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();
            DataLoaded?.Invoke(this, new OverlayLogDataLoadedEventArgs(loaded));
        }
    }

    /// <summary>Returns the current in-memory entries. Safe to call at any time.</summary>
    public List<OverlayLogEntryRecord> GetEntries() => _entries;

    /// <summary>Suppresses UI events and saves during a bulk journal scan.</summary>
    public void BeginBulkLoad() => _isBulkLoading = true;

    /// <summary>Ends bulk load mode and fires DataLoaded if entries were accumulated.</summary>
    public void EndBulkLoad()
    {
        _isBulkLoading = false;
        if (_entries.Count > 0)
            DataLoaded?.Invoke(this, new OverlayLogDataLoadedEventArgs(_entries.ToList()));
        ScheduleSave();
    }

    public void HandleEvent(EventBase evt)
    {
        if (evt.Timestamp <= _lastEventTimestamp) return;

        if (evt is ScanEvent scan)
            HandleScanEvent(scan);
        else if (evt is FSSBodySignalsEvent signals)
            HandleFSSBodySignalsEvent(signals);
        else if (evt is ScanOrganicEvent scanOrganic)
            HandleScanOrganicEvent(scanOrganic);
    }

    private void HandleScanEvent(ScanEvent evt)
    {
        if (evt.ScanType != "Detailed") return;
        if (!string.IsNullOrWhiteSpace(evt.StarType)) return;

        bool isValuable = BodyValueHelper.IsEarthLikeWorld(evt.PlanetClass)
                       || BodyValueHelper.IsWaterWorld(evt.PlanetClass)
                       || evt.PlanetClass.Contains("Ammonia", StringComparison.OrdinalIgnoreCase);

        if (!isValuable && BodyValueHelper.HasTerraformState(evt.TerraformState))
            isValuable = true;

        if (isValuable)
            AddBodyScanEntry(evt.BodyName, evt.PlanetClass, evt.TerraformState,
                evt.WasDiscovered ?? false, evt.WasMapped ?? false, evt.Timestamp);
    }

    private void HandleFSSBodySignalsEvent(FSSBodySignalsEvent evt)
    {
        int biologicalSignalCount = BodyValueHelper.GetBiologicalSignalCount(evt.Signals);
        bool hasBio = biologicalSignalCount > 0;

        if (!hasBio) return;

        // FSSBodySignalsEvent includes BodyName and fires as soon as the FSS detects signals —
        // no need to wait for the detailed ScanEvent.
        AddBodyScanEntry(evt.BodyName, $"Biological ({biologicalSignalCount})", string.Empty, false, false, evt.Timestamp);
    }

    private void AddBodyScanEntry(string bodyName, string planetClass, string? terraformState,
        bool wasDiscovered, bool wasMapped, DateTime timestamp)
    {
        var subParts = new List<string> { planetClass };
        if (!string.IsNullOrWhiteSpace(terraformState))
            subParts.Add("Terraform");
        if (!wasDiscovered) subParts.Add("First Discovery");
        if (!wasMapped) subParts.Add("Unmapped");

        var record = new OverlayLogEntryRecord
        {
            EntryType = nameof(OverlayLogEntryType.BodyScan),
            TimeText = timestamp.ToString("HH:mm"),
            Title = bodyName,
            SubText = string.Join(" · ", subParts),
            ValueText = string.Empty,
            Time = timestamp
            
        };

        AddEntry(record);
        if (!_isBulkLoading)
        {
            EntryAdded?.Invoke(this, new OverlayLogEntryEventArgs(record));
        }
        ScheduleSave();
    }

    private void HandleScanOrganicEvent(ScanOrganicEvent evt)
    {
        string key = $"{evt.Genus}|{evt.Variant}|{evt.SystemAddress}|{evt.Body}";
        var existing = _entries.FirstOrDefault(r => r.Key == key);
        if (existing != null) return;

        long sampleValue = ExobiologyValues.GetValue(evt.Species_Localised);

        var now = DateTime.Now;
        var record = new OverlayLogEntryRecord
        {
            EntryType = nameof(OverlayLogEntryType.ExoBio),
            Key = key,
            TimeText = evt.Timestamp.ToString("HH:mm"),
            Title = evt.Genus_Localised ?? evt.Genus,
            SubText = FormatScanType(evt.ScanType),
            ValueText = sampleValue > 0 ? $"~{sampleValue:N0} CR" : string.Empty,
            Time = evt.Timestamp
        };


        AddEntry(record);
        if (!_isBulkLoading)
        {
            EntryAdded?.Invoke(this, new OverlayLogEntryEventArgs(record));
        }
        ScheduleSave();
    }

    private void AddEntry(OverlayLogEntryRecord record)
    {
        _entries.Add(record);
        while (_entries.Count > MaxEntries)
            _entries.RemoveAt(0);
        if (record.Time.HasValue && record.Time.Value > _lastEventTimestamp)
            _lastEventTimestamp = record.Time.Value;
    }

    private void ScheduleSave()
    {
        lock (_saveLock)
        {
            _lastSaveRequest = DateTime.UtcNow;
            if (_saveTask == null || _saveTask.IsCompleted)
                _saveTask = DebouncedSaveAsync();
        }
    }

    private async Task DebouncedSaveAsync()
    {
        while (true)
        {
            DateTime requestTime;
            lock (_saveLock) { requestTime = _lastSaveRequest; }

            var remaining = _saveDebounce - (DateTime.UtcNow - requestTime);
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining);
            else
            {
                lock (_saveLock)
                {
                    if (DateTime.UtcNow - _lastSaveRequest >= _saveDebounce)
                        break;
                }
            }
        }

        await _dataService.SaveDataAsync(_entries);
    }

    private static string FormatScanType(string scanType) => scanType switch
    {
        "Log"     => "Logged",
        "Sample"  => "Sampled",
        "Analyse" => "Analysed",
        _         => scanType
    };
}

public class OverlayLogDataLoadedEventArgs : EventArgs
{
    public List<OverlayLogEntryRecord> Entries { get; }
    public OverlayLogDataLoadedEventArgs(List<OverlayLogEntryRecord> entries) => Entries = entries;
}

public class OverlayLogEntryEventArgs : EventArgs
{
    public OverlayLogEntryRecord Entry { get; }
    public OverlayLogEntryEventArgs(OverlayLogEntryRecord entry) => Entry = entry;
}

/// <summary>Marker enum so the service can write the correct EntryType string into records.</summary>
public enum OverlayLogEntryType { BodyScan, ExoBio }
