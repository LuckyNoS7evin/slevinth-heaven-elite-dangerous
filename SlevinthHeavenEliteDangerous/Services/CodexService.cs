using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Tracks first-discovery codex entries from journal events.
/// Only entries where IsNewEntry is true are recorded.
/// </summary>
public class CodexService : IEventHandler
{
    private readonly CodexDataService _dataService = new();
    private readonly Dictionary<string, CodexEntryModel> _entries = [];
    private bool _isLoading;
    private DateTime _lastSaveRequest = DateTime.MinValue;
    private readonly TimeSpan _saveDebounceDelay = TimeSpan.FromMilliseconds(500);
    private Task? _saveTask;
    private readonly object _saveLock = new();

    public event EventHandler<CodexEntryAddedEventArgs>? EntryAdded;
    public event EventHandler<CodexDataLoadedEventArgs>? DataLoaded;

    public void HandleEvent(EventBase evt)
    {
        if (evt is CodexEntryEvent codex)
            HandleCodexEntryEvent(codex);
    }

    private void HandleCodexEntryEvent(CodexEntryEvent evt)
    {
        if (evt.IsNewEntry != true)
            return;

        if (!evt.EntryID.HasValue)
            return;

        // Deduplicate by EntryID + Region (same species can be a first discovery in multiple regions)
        var key = $"{evt.EntryID}|{evt.Region}";

        if (_entries.ContainsKey(key))
            return;

        var entry = new CodexEntryModel
        {
            EntryID     = evt.EntryID.Value,
            Name        = !string.IsNullOrEmpty(evt.Name_Localised) ? evt.Name_Localised : evt.Name,
            Category    = !string.IsNullOrEmpty(evt.Category_Localised) ? evt.Category_Localised : evt.Category,
            SubCategory = !string.IsNullOrEmpty(evt.SubCategory_Localised) ? evt.SubCategory_Localised : evt.SubCategory,
            Region      = !string.IsNullOrEmpty(evt.Region_Localised) ? evt.Region_Localised : evt.Region,
            System      = evt.System,
            Timestamp   = evt.Timestamp,
            VoucherAmount = evt.VoucherAmount
        };

        _entries[key] = entry;

        System.Diagnostics.Debug.WriteLine(
            $"[CodexService] New entry: {entry.Name} ({entry.Category} / {entry.SubCategory}) in {entry.Region}");

        EntryAdded?.Invoke(this, new CodexEntryAddedEventArgs(entry));
        ScheduleSave();
    }

    public int TotalEntries => _entries.Count;

    public async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            var data = await _dataService.LoadDataAsync();
            if (data != null)
            {
                _entries.Clear();
                foreach (var entry in data.Entries)
                {
                    var key = $"{entry.EntryID}|{entry.Region}";
                    _entries[key] = entry;
                }

                System.Diagnostics.Debug.WriteLine($"[CodexService] Loaded {_entries.Count} codex entries.");
                DataLoaded?.Invoke(this, new CodexDataLoadedEventArgs([.. _entries.Values]));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CodexService] No existing codex data, starting fresh.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CodexService] Error loading data: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    public void ScheduleSave()
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
            DateTime saveRequestTime;
            lock (_saveLock) { saveRequestTime = _lastSaveRequest; }

            var remaining = _saveDebounceDelay - (DateTime.UtcNow - saveRequestTime);
            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining);
            }
            else
            {
                lock (_saveLock)
                {
                    if (DateTime.UtcNow - _lastSaveRequest >= _saveDebounceDelay)
                        break;
                }
            }
        }

        if (_isLoading) return;

        try
        {
            var state = new CodexStateModel
            {
                Entries = [.. _entries.Values.OrderBy(e => e.Timestamp)]
            };
            await _dataService.SaveDataAsync(state);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CodexService] Error saving: {ex.Message}");
        }
    }
}

public class CodexEntryAddedEventArgs : EventArgs
{
    public CodexEntryModel Entry { get; }
    public CodexEntryAddedEventArgs(CodexEntryModel entry) => Entry = entry;
}

public class CodexDataLoadedEventArgs : EventArgs
{
    public IReadOnlyList<CodexEntryModel> Entries { get; }
    public CodexDataLoadedEventArgs(IReadOnlyList<CodexEntryModel> entries) => Entries = entries;
}
