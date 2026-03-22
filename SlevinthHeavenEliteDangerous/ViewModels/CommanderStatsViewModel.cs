using SlevinthHeavenEliteDangerous.Services;
using Microsoft.UI.Dispatching;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SlevinthHeavenEliteDangerous.Services.Models;

namespace SlevinthHeavenEliteDangerous.ViewModels;

public sealed class CommanderStatsViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly CommanderStatsService _service;

    // Bank Account
    private long _currentWealth;
    private int _ownedShipCount;

    // Exploration
    private int _systemsVisited;
    private long _explorationProfits;
    private int _totalHyperspaceJumps;
    private double _totalHyperspaceDistance;
    private long _timePlayed;

    // Trading
    private long _marketProfits;
    private int _marketsTradedWith;

    // Combat
    private int _bountiesClaimed;
    private long _bountyHuntingProfit;

    // Mining
    private long _miningProfits;

    // Exobiology
    private long _exobiologyProfits;
    private int _organicSpeciesAnalysed;

    public event PropertyChangedEventHandler? PropertyChanged;

    public CommanderStatsViewModel(DispatcherQueue dispatcherQueue, CommanderStatsService service)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        _service.StatsUpdated += OnStatsUpdated;
        UpdateFromModel(_service.GetStats());
    }

    public void Dispose() => _service.StatsUpdated -= OnStatsUpdated;

    private void OnStatsUpdated(object? sender, CommanderStatsUpdatedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() => UpdateFromModel(e.Stats));
    }

    private void UpdateFromModel(CommanderStatsModel m)
    {
        CurrentWealth           = m.CurrentWealth;
        OwnedShipCount          = m.OwnedShipCount;
        SystemsVisited          = m.SystemsVisited;
        ExplorationProfits      = m.ExplorationProfits;
        TotalHyperspaceJumps    = m.TotalHyperspaceJumps;
        TotalHyperspaceDistance = m.TotalHyperspaceDistance;
        TimePlayed              = m.TimePlayed;
        MarketProfits           = m.MarketProfits;
        MarketsTradedWith       = m.MarketsTradedWith;
        BountiesClaimed         = m.BountiesClaimed;
        BountyHuntingProfit     = m.BountyHuntingProfit;
        MiningProfits           = m.MiningProfits;
        ExobiologyProfits       = m.ExobiologyProfits;
        OrganicSpeciesAnalysed  = m.OrganicSpeciesAnalysed;
    }

    // --- Bank Account ---
    public long CurrentWealth
    {
        get => _currentWealth;
        private set { if (_currentWealth != value) { _currentWealth = value; OnPropertyChanged(); OnPropertyChanged(nameof(CurrentWealthFormatted)); } }
    }
    public int OwnedShipCount
    {
        get => _ownedShipCount;
        private set { if (_ownedShipCount != value) { _ownedShipCount = value; OnPropertyChanged(); } }
    }
    public string CurrentWealthFormatted => $"{CurrentWealth:N0} CR";

    // --- Exploration ---
    public int SystemsVisited
    {
        get => _systemsVisited;
        private set { if (_systemsVisited != value) { _systemsVisited = value; OnPropertyChanged(); OnPropertyChanged(nameof(SystemsVisitedFormatted)); } }
    }
    public long ExplorationProfits
    {
        get => _explorationProfits;
        private set { if (_explorationProfits != value) { _explorationProfits = value; OnPropertyChanged(); OnPropertyChanged(nameof(ExplorationProfitsFormatted)); } }
    }
    public int TotalHyperspaceJumps
    {
        get => _totalHyperspaceJumps;
        private set { if (_totalHyperspaceJumps != value) { _totalHyperspaceJumps = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalHyperspaceJumpsFormatted)); } }
    }
    public double TotalHyperspaceDistance
    {
        get => _totalHyperspaceDistance;
        private set { if (_totalHyperspaceDistance != value) { _totalHyperspaceDistance = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalHyperspaceDistanceFormatted)); } }
    }
    public long TimePlayed
    {
        get => _timePlayed;
        private set { if (_timePlayed != value) { _timePlayed = value; OnPropertyChanged(); OnPropertyChanged(nameof(TimePlayedFormatted)); } }
    }
    public string SystemsVisitedFormatted          => $"{SystemsVisited:N0}";
    public string ExplorationProfitsFormatted       => $"{ExplorationProfits:N0} CR";
    public string TotalHyperspaceJumpsFormatted     => $"{TotalHyperspaceJumps:N0}";
    public string TotalHyperspaceDistanceFormatted  => $"{TotalHyperspaceDistance:N0} Ly";
    public string TimePlayedFormatted
    {
        get
        {
            var ts = TimeSpan.FromSeconds(TimePlayed);
            return ts.TotalDays >= 1
                ? $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m"
                : ts.TotalHours >= 1
                    ? $"{ts.Hours}h {ts.Minutes}m"
                    : $"{ts.Minutes}m";
        }
    }

    // --- Trading ---
    public long MarketProfits
    {
        get => _marketProfits;
        private set { if (_marketProfits != value) { _marketProfits = value; OnPropertyChanged(); OnPropertyChanged(nameof(MarketProfitsFormatted)); } }
    }
    public int MarketsTradedWith
    {
        get => _marketsTradedWith;
        private set { if (_marketsTradedWith != value) { _marketsTradedWith = value; OnPropertyChanged(); } }
    }
    public string MarketProfitsFormatted => $"{MarketProfits:N0} CR";

    // --- Combat ---
    public int BountiesClaimed
    {
        get => _bountiesClaimed;
        private set { if (_bountiesClaimed != value) { _bountiesClaimed = value; OnPropertyChanged(); OnPropertyChanged(nameof(BountiesClaimedFormatted)); } }
    }
    public long BountyHuntingProfit
    {
        get => _bountyHuntingProfit;
        private set { if (_bountyHuntingProfit != value) { _bountyHuntingProfit = value; OnPropertyChanged(); OnPropertyChanged(nameof(BountyHuntingProfitFormatted)); } }
    }
    public string BountiesClaimedFormatted      => $"{BountiesClaimed:N0}";
    public string BountyHuntingProfitFormatted  => $"{BountyHuntingProfit:N0} CR";

    // --- Mining ---
    public long MiningProfits
    {
        get => _miningProfits;
        private set { if (_miningProfits != value) { _miningProfits = value; OnPropertyChanged(); OnPropertyChanged(nameof(MiningProfitsFormatted)); } }
    }
    public string MiningProfitsFormatted => $"{MiningProfits:N0} CR";

    // --- Exobiology ---
    public long ExobiologyProfits
    {
        get => _exobiologyProfits;
        private set { if (_exobiologyProfits != value) { _exobiologyProfits = value; OnPropertyChanged(); OnPropertyChanged(nameof(ExobiologyProfitsFormatted)); } }
    }
    public int OrganicSpeciesAnalysed
    {
        get => _organicSpeciesAnalysed;
        private set { if (_organicSpeciesAnalysed != value) { _organicSpeciesAnalysed = value; OnPropertyChanged(); } }
    }
    public string ExobiologyProfitsFormatted => $"{ExobiologyProfits:N0} CR";

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
