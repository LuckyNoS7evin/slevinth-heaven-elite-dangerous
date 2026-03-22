using Microsoft.UI.Dispatching;
using SlevinthHeavenEliteDangerous.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for commander ranks display
/// </summary>
public class RanksViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly RankService _rankService;

    public ObservableCollection<RankItemViewModel> Ranks { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public RanksViewModel(DispatcherQueue dispatcherQueue, RankService rankService)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _rankService = rankService ?? throw new ArgumentNullException(nameof(rankService));

        InitializeRanks();

        _rankService.RankUpdated += OnRankUpdated;
        _rankService.ProgressUpdated += OnProgressUpdated;
    }

    private void InitializeRanks()
    {
        Ranks.Add(new RankItemViewModel { RankType = "Combat" });
        Ranks.Add(new RankItemViewModel { RankType = "Trade" });
        Ranks.Add(new RankItemViewModel { RankType = "Explore" });
        Ranks.Add(new RankItemViewModel { RankType = "Soldier" });
        Ranks.Add(new RankItemViewModel { RankType = "Exobiologist" });
        Ranks.Add(new RankItemViewModel { RankType = "Empire" });
        Ranks.Add(new RankItemViewModel { RankType = "Federation" });
        Ranks.Add(new RankItemViewModel { RankType = "CQC" });
    }

    private void OnRankUpdated(object? sender, RankUpdatedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            for (int i = 0; i < Ranks.Count && i < e.Ranks.Count; i++)
            {
                Ranks[i].UpdateFrom(e.Ranks[i]);
            }
        });
    }

    private void OnProgressUpdated(object? sender, ProgressUpdatedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            for (int i = 0; i < Ranks.Count && i < e.Ranks.Count; i++)
            {
                Ranks[i].Progress = e.Ranks[i].Progress;
            }
        });
    }

    public void Dispose()
    {
        _rankService.RankUpdated -= OnRankUpdated;
        _rankService.ProgressUpdated -= OnProgressUpdated;
    }
}
