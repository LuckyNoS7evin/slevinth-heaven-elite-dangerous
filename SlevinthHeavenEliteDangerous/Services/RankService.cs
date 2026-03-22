using SlevinthHeavenEliteDangerous.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Services.Models;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Service for tracking commander ranks from journal events
/// </summary>
public class RankService : IEventHandler
{
    private readonly RankDataService _dataService = new();
    private bool _isLoading;

    // Fixed-order list — indices are stable so ViewModel can update by index
    private readonly List<RankModel> _ranks =
    [
        new() { RankType = "Combat" },
        new() { RankType = "Trade" },
        new() { RankType = "Explore" },
        new() { RankType = "Soldier" },
        new() { RankType = "Exobiologist" },
        new() { RankType = "Empire" },
        new() { RankType = "Federation" },
        new() { RankType = "CQC" },
    ];

    public event EventHandler<RankUpdatedEventArgs>? RankUpdated;
    public event EventHandler<ProgressUpdatedEventArgs>? ProgressUpdated;

    public void HandleEvent(EventBase evt)
    {
        switch (evt)
        {
            case RankEvent rankEvt:
                HandleRankEvent(rankEvt);
                break;
            case ProgressEvent progressEvt:
                HandleProgressEvent(progressEvt);
                break;
            case PromotionEvent promotionEvt:
                HandlePromotionEvent(promotionEvt);
                break;
        }
    }

    private void HandleRankEvent(RankEvent evt)
    {
        System.Diagnostics.Debug.WriteLine($"[RankService] HandleRankEvent - Combat: {evt.Combat}, Trade: {evt.Trade}, Explore: {evt.Explore}");

        _ranks[0].RankValue = evt.Combat;
        _ranks[1].RankValue = evt.Trade;
        _ranks[2].RankValue = evt.Explore;
        _ranks[3].RankValue = evt.Soldier;
        _ranks[4].RankValue = evt.Exobiologist;
        _ranks[5].RankValue = evt.Empire;
        _ranks[6].RankValue = evt.Federation;
        _ranks[7].RankValue = evt.CQC;

        RankUpdated?.Invoke(this, new RankUpdatedEventArgs(_ranks));
        ScheduleSave();
    }

    private void HandleProgressEvent(ProgressEvent evt)
    {
        System.Diagnostics.Debug.WriteLine($"[RankService] HandleProgressEvent - Combat: {evt.Combat}%, Trade: {evt.Trade}%, Explore: {evt.Explore}%");

        _ranks[0].Progress = evt.Combat;
        _ranks[1].Progress = evt.Trade;
        _ranks[2].Progress = evt.Explore;
        _ranks[3].Progress = evt.Soldier;
        _ranks[4].Progress = evt.Exobiologist;
        _ranks[5].Progress = evt.Empire;
        _ranks[6].Progress = evt.Federation;
        _ranks[7].Progress = evt.CQC;

        ProgressUpdated?.Invoke(this, new ProgressUpdatedEventArgs(_ranks));
        ScheduleSave();
    }

    private void HandlePromotionEvent(PromotionEvent evt)
    {
        System.Diagnostics.Debug.WriteLine($"[RankService] HandlePromotionEvent - Combat: {evt.Combat}, Trade: {evt.Trade}, Explore: {evt.Explore}");

        if (evt.Combat.HasValue) { _ranks[0].RankValue = evt.Combat.Value; _ranks[0].Progress = 0; }
        if (evt.Trade.HasValue) { _ranks[1].RankValue = evt.Trade.Value; _ranks[1].Progress = 0; }
        if (evt.Explore.HasValue) { _ranks[2].RankValue = evt.Explore.Value; _ranks[2].Progress = 0; }
        if (evt.Soldier.HasValue) { _ranks[3].RankValue = evt.Soldier.Value; _ranks[3].Progress = 0; }
        if (evt.Exobiologist.HasValue) { _ranks[4].RankValue = evt.Exobiologist.Value; _ranks[4].Progress = 0; }
        if (evt.Empire.HasValue) { _ranks[5].RankValue = evt.Empire.Value; _ranks[5].Progress = 0; }
        if (evt.Federation.HasValue) { _ranks[6].RankValue = evt.Federation.Value; _ranks[6].Progress = 0; }
        if (evt.CQC.HasValue) { _ranks[7].RankValue = evt.CQC.Value; _ranks[7].Progress = 0; }

        RankUpdated?.Invoke(this, new RankUpdatedEventArgs(_ranks));
        ScheduleSave();
    }

    public async System.Threading.Tasks.Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            var data = await _dataService.LoadDataAsync();
            if (data != null)
            {
                foreach (var savedRank in data)
                {
                    var rank = _ranks.FirstOrDefault(r => r.RankType == savedRank.RankType);
                    if (rank != null)
                    {
                        rank.RankValue = savedRank.RankValue;
                        rank.Progress = savedRank.Progress;
                    }
                }
                System.Diagnostics.Debug.WriteLine("[RankService] Loaded rank data from ranks_data.json");
                RankUpdated?.Invoke(this, new RankUpdatedEventArgs(_ranks));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[RankService] No existing rank data found, starting fresh");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RankService] Error loading data: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void ScheduleSave()
    {
        if (_isLoading) return;

        try
        {
            System.Diagnostics.Debug.WriteLine("[RankService] Saving ranks");
            var snapshot = _ranks.Select(r => new RankModel
            {
                RankType = r.RankType,
                RankValue = r.RankValue,
                Progress = r.Progress
            }).ToList();

            await _dataService.SaveDataAsync(snapshot);
            System.Diagnostics.Debug.WriteLine("[RankService] Save completed successfully to ranks_data.json");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RankService] Error saving: {ex.Message}");
        }
    }
}

/// <summary>
/// Event args for rank updates
/// </summary>
public class RankUpdatedEventArgs : EventArgs
{
    public IReadOnlyList<RankModel> Ranks { get; }

    public RankUpdatedEventArgs(List<RankModel> ranks)
    {
        Ranks = ranks;
    }
}

/// <summary>
/// Event args for progress updates
/// </summary>
public class ProgressUpdatedEventArgs : EventArgs
{
    public IReadOnlyList<RankModel> Ranks { get; }

    public ProgressUpdatedEventArgs(List<RankModel> ranks)
    {
        Ranks = ranks;
    }
}
