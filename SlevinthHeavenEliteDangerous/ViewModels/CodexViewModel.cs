using Microsoft.UI.Dispatching;
using SlevinthHeavenEliteDangerous.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for the Codex tab — lists first-discovery entries, newest first.
/// </summary>
public class CodexViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly CodexService _service;
    private int _totalEntries;

    public ObservableCollection<CodexEntryViewModel> Entries { get; } = [];

    public int TotalEntries
    {
        get => _totalEntries;
        private set
        {
            if (_totalEntries != value)
            {
                _totalEntries = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalEntriesFormatted));
            }
        }
    }

    public string TotalEntriesFormatted => $"{TotalEntries:N0} first discoveries";

    public event PropertyChangedEventHandler? PropertyChanged;

    public CodexViewModel(DispatcherQueue dispatcherQueue, CodexService service)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        _service.EntryAdded  += OnEntryAdded;
        _service.DataLoaded  += OnDataLoaded;
    }

    public void Dispose()
    {
        _service.EntryAdded -= OnEntryAdded;
        _service.DataLoaded -= OnDataLoaded;
    }

    private void OnEntryAdded(object? sender, CodexEntryAddedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            Entries.Insert(0, CodexEntryViewModel.FromModel(e.Entry));
            TotalEntries = Entries.Count;
        });
    }

    private void OnDataLoaded(object? sender, CodexDataLoadedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            Entries.Clear();
            // Loaded list is already ordered oldest→newest; insert reversed for newest-first display
            for (int i = e.Entries.Count - 1; i >= 0; i--)
                Entries.Add(CodexEntryViewModel.FromModel(e.Entries[i]));

            TotalEntries = Entries.Count;
        });
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
