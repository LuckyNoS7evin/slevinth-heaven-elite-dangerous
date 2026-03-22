using Microsoft.UI.Dispatching;
using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.Services.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for the Reputation tab — exposes four faction reputation cards.
/// </summary>
public class ReputationViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ReputationService _service;

    public ObservableCollection<FactionReputationViewModel> Factions { get; } =
    [
        new() { FactionName = "Empire" },
        new() { FactionName = "Federation" },
        new() { FactionName = "Independent" },
        new() { FactionName = "Alliance" }
    ];

    public event PropertyChangedEventHandler? PropertyChanged;

    public ReputationViewModel(DispatcherQueue dispatcherQueue, ReputationService service)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        _service.ReputationUpdated += OnReputationUpdated;
        UpdateFromModel(_service.GetReputation());
    }

    public void Dispose() => _service.ReputationUpdated -= OnReputationUpdated;

    private void OnReputationUpdated(object? sender, ReputationUpdatedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() => UpdateFromModel(e.Reputation));
    }

    private void UpdateFromModel(ReputationModel m)
    {
        Factions[0].Value = m.Empire;
        Factions[1].Value = m.Federation;
        Factions[2].Value = m.Independent;
        Factions[3].Value = m.Alliance;
    }
}
