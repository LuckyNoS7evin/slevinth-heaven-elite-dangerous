using SlevinthHeavenEliteDangerous.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SlevinthHeavenEliteDangerous.Services.Models;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for the overlay scan log — body scans and exobio discoveries.
/// Read-only from the service: subscribes to events and displays entries.
/// </summary>
public sealed class OverlayLogViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly OverlayLogService _overlayLogService;
    private readonly DispatcherQueue _dispatcherQueue;

    public ObservableCollection<OverlayLogEntry> Entries { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public OverlayLogViewModel(DispatcherQueue dispatcherQueue, OverlayLogService overlayLogService)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _overlayLogService = overlayLogService ?? throw new ArgumentNullException(nameof(overlayLogService));

        _overlayLogService.DataLoaded += OnDataLoaded;
        _overlayLogService.EntryAdded += OnEntryAdded;

        // If LoadAsync already fired before this VM was created, populate from the in-memory list.
        // Queue via TryEnqueue so it runs after the control finishes wiring up ItemsSource.
        _dispatcherQueue.TryEnqueue(() =>
        {
            if (Entries.Count == 0)
            {
                var existing = _overlayLogService.GetEntries();
                if (existing.Count > 0)
                    PopulateFromRecords(existing);
            }
        });
    }

    public void Dispose()
    {
        _overlayLogService.DataLoaded -= OnDataLoaded;
        _overlayLogService.EntryAdded -= OnEntryAdded;
    }

    private void OnDataLoaded(object? sender, OverlayLogDataLoadedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            Entries.Clear();
            PopulateFromRecords(e.Entries);
        });
    }

    private void OnEntryAdded(object? sender, OverlayLogEntryEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() => Entries.Insert(0, RecordToEntry(e.Entry)));
    }

    private void OnEntryUpdated(object? sender, OverlayLogEntryEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var existing = Entries.FirstOrDefault(x => x.Key == e.Entry.Key);
            if (existing == null) return;

            var idx = Entries.IndexOf(existing);
            Entries[idx] = new OverlayLogEntry
            {
                EntryType = existing.EntryType,
                Key = existing.Key,
                TimeText = existing.TimeText,
                Title = existing.Title,
                SubText = e.Entry.SubText,
                ValueText = e.Entry.ValueText
            };
        });
    }

    private void PopulateFromRecords(System.Collections.Generic.List<OverlayLogEntryRecord> records)
    {
        foreach (var record in records)
            Entries.Insert(0, RecordToEntry(record));
    }

    private static OverlayLogEntry RecordToEntry(OverlayLogEntryRecord r) => new OverlayLogEntry
    {
        EntryType = r.EntryType == nameof(OverlayLogEntryType.ExoBio)
            ? OverlayLogEntryType.ExoBio
            : OverlayLogEntryType.BodyScan,
        TimeText = r.TimeText,
        Title = r.Title,
        SubText = r.SubText,
        ValueText = r.ValueText,
        Key = r.Key
    };
}
