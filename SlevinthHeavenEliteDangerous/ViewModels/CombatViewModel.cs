using Microsoft.UI.Dispatching;
using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for the Combat Log tab.
/// Combines CombatService (kill/bounty/crime data) with RankService (combat rank + progress)
/// to expose stats and the estimated kills-to-next-rank table.
/// </summary>
public sealed class CombatViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly CombatService _combatService;
    private readonly RankService _rankService;

    // ── Backing fields ───────────────────────────────────────────────────────

    private int _npcKillCount;
    private int _pvpKillCount;
    private long _totalBountyCredits;
    private long _totalCombatBondCredits;
    private long _totalCapShipBondCredits;
    private int _deathCount;
    private int _interdictionsAttempted;
    private int _interdictionsSucceeded;
    private int _timesInterdicted;
    private int _interdictionsEscaped;
    private double _hullLowWaterMark = 1.0;
    private int _crimeCount;
    private int _combatRank;
    private int _combatRankProgress;

    // ── Observable collections ───────────────────────────────────────────────

    public ObservableCollection<CombatKillEntryViewModel> KillLog { get; } = [];
    public ObservableCollection<FactionKillViewModel> TopFactions { get; } = [];

    // ── Kill count properties ────────────────────────────────────────────────

    public int NpcKillCount
    {
        get => _npcKillCount;
        private set { if (_npcKillCount != value) { _npcKillCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalKillCount)); } }
    }

    public int PvpKillCount
    {
        get => _pvpKillCount;
        private set { if (_pvpKillCount != value) { _pvpKillCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalKillCount)); } }
    }

    public int TotalKillCount => NpcKillCount + PvpKillCount;

    // ── Credits properties ───────────────────────────────────────────────────

    public long TotalBountyCredits
    {
        get => _totalBountyCredits;
        private set { if (_totalBountyCredits != value) { _totalBountyCredits = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalBountyCreditsFormatted)); OnPropertyChanged(nameof(TotalCombatEarningsFormatted)); } }
    }
    public string TotalBountyCreditsFormatted => _totalBountyCredits.ToString("N0") + " CR";

    public long TotalCombatBondCredits
    {
        get => _totalCombatBondCredits;
        private set { if (_totalCombatBondCredits != value) { _totalCombatBondCredits = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalCombatBondCreditsFormatted)); OnPropertyChanged(nameof(TotalCombatEarningsFormatted)); } }
    }
    public string TotalCombatBondCreditsFormatted => _totalCombatBondCredits.ToString("N0") + " CR";

    public long TotalCapShipBondCredits
    {
        get => _totalCapShipBondCredits;
        private set { if (_totalCapShipBondCredits != value) { _totalCapShipBondCredits = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalCapShipBondCreditsFormatted)); OnPropertyChanged(nameof(TotalCombatEarningsFormatted)); } }
    }
    public string TotalCapShipBondCreditsFormatted => _totalCapShipBondCredits.ToString("N0") + " CR";

    public string TotalCombatEarningsFormatted =>
        (_totalBountyCredits + _totalCombatBondCredits + _totalCapShipBondCredits).ToString("N0") + " CR";

    // ── Misc stats ───────────────────────────────────────────────────────────

    public int DeathCount
    {
        get => _deathCount;
        private set { if (_deathCount != value) { _deathCount = value; OnPropertyChanged(); } }
    }

    public int InterdictionsAttempted
    {
        get => _interdictionsAttempted;
        private set { if (_interdictionsAttempted != value) { _interdictionsAttempted = value; OnPropertyChanged(); } }
    }

    public int InterdictionsSucceeded
    {
        get => _interdictionsSucceeded;
        private set { if (_interdictionsSucceeded != value) { _interdictionsSucceeded = value; OnPropertyChanged(); } }
    }

    public int TimesInterdicted
    {
        get => _timesInterdicted;
        private set { if (_timesInterdicted != value) { _timesInterdicted = value; OnPropertyChanged(); } }
    }

    public int InterdictionsEscaped
    {
        get => _interdictionsEscaped;
        private set { if (_interdictionsEscaped != value) { _interdictionsEscaped = value; OnPropertyChanged(); } }
    }

    public double HullLowWaterMark
    {
        get => _hullLowWaterMark;
        private set { if (_hullLowWaterMark != value) { _hullLowWaterMark = value; OnPropertyChanged(); OnPropertyChanged(nameof(HullLowWaterMarkFormatted)); } }
    }
    public string HullLowWaterMarkFormatted => $"{_hullLowWaterMark * 100:F0} %";

    public int CrimeCount
    {
        get => _crimeCount;
        private set { if (_crimeCount != value) { _crimeCount = value; OnPropertyChanged(); } }
    }

    // ── Rank properties ──────────────────────────────────────────────────────

    public int CombatRank
    {
        get => _combatRank;
        private set
        {
            if (_combatRank != value)
            {
                _combatRank = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CombatRankName));
                OnPropertyChanged(nameof(NextRankName));
                OnPropertyChanged(nameof(IsAtMaxRank));
                NotifyRankEstimatesChanged();
            }
        }
    }

    public int CombatRankProgress
    {
        get => _combatRankProgress;
        private set
        {
            if (_combatRankProgress != value)
            {
                _combatRankProgress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CombatRankProgressFormatted));
                NotifyRankEstimatesChanged();
            }
        }
    }

    public string CombatRankName => CombatRankHelper.GetRankName(_combatRank);
    public string NextRankName   => CombatRankHelper.GetNextRankName(_combatRank);
    public string CombatRankProgressFormatted => $"{_combatRankProgress} %";
    public bool IsAtMaxRank      => _combatRank >= 8;

    // ── Kills-to-next-rank estimates (at different victim ranks) ─────────────

    /// <summary>Estimated kills to next rank fighting an opponent 2 ranks above the player.</summary>
    public string KillsEstimatePlus2 => FormatKillEstimate(_combatRank, _combatRankProgress, _combatRank + 2);

    /// <summary>Estimated kills to next rank fighting an opponent 1 rank above the player.</summary>
    public string KillsEstimatePlus1 => FormatKillEstimate(_combatRank, _combatRankProgress, _combatRank + 1);

    /// <summary>Estimated kills to next rank fighting an opponent of equal rank.</summary>
    public string KillsEstimateEqual => FormatKillEstimate(_combatRank, _combatRankProgress, _combatRank);

    /// <summary>Estimated kills to next rank fighting an opponent 1 rank below the player.</summary>
    public string KillsEstimateMinus1 => FormatKillEstimate(_combatRank, _combatRankProgress, _combatRank - 1);

    /// <summary>Estimated kills to next rank fighting an opponent 2 ranks below the player.</summary>
    public string KillsEstimateMinus2 => FormatKillEstimate(_combatRank, _combatRankProgress, _combatRank - 2);

    private static string FormatKillEstimate(int yourRank, int progress, int victimRank)
    {
        if (yourRank >= 8) return "—";
        int clampedVictim = Math.Max(0, Math.Min(8, victimRank));
        int? estimate = CombatRankHelper.EstimateKillsToNextRank(yourRank, progress, clampedVictim);
        return estimate.HasValue ? estimate.Value.ToString("N0") : "—";
    }

    // ── Constructor ──────────────────────────────────────────────────────────

    public CombatViewModel(DispatcherQueue dispatcherQueue, CombatService combatService, RankService rankService)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _combatService   = combatService   ?? throw new ArgumentNullException(nameof(combatService));
        _rankService     = rankService     ?? throw new ArgumentNullException(nameof(rankService));

        _combatService.KillLogged   += OnKillLogged;
        _combatService.StatsChanged += OnStatsChanged;
        _combatService.DataLoaded   += OnDataLoaded;

        _rankService.RankUpdated     += OnRankUpdated;
        _rankService.ProgressUpdated += OnProgressUpdated;
    }

    public void Dispose()
    {
        _combatService.KillLogged   -= OnKillLogged;
        _combatService.StatsChanged -= OnStatsChanged;
        _combatService.DataLoaded   -= OnDataLoaded;

        _rankService.RankUpdated     -= OnRankUpdated;
        _rankService.ProgressUpdated -= OnProgressUpdated;
    }

    // ── Service event handlers ────────────────────────────────────────────────

    private void OnKillLogged(object? sender, CombatKillEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            KillLog.Insert(0, CombatKillEntryViewModel.FromRecord(e.Record));
        });
    }

    private void OnStatsChanged(object? sender, CombatStatsChangedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() => ApplyState(e.State));
    }

    private void OnDataLoaded(object? sender, CombatDataLoadedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            ApplyState(e.State);

            KillLog.Clear();
            foreach (var record in e.State.KillLog)
                KillLog.Add(CombatKillEntryViewModel.FromRecord(record));
        });
    }

    private void OnRankUpdated(object? sender, RankUpdatedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var combat = e.Ranks.FirstOrDefault(r => r.RankType == "Combat");
            if (combat != null) CombatRank = combat.RankValue;
        });
    }

    private void OnProgressUpdated(object? sender, ProgressUpdatedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var combat = e.Ranks.FirstOrDefault(r => r.RankType == "Combat");
            if (combat != null) CombatRankProgress = combat.Progress;
        });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void ApplyState(CombatStateModel s)
    {
        NpcKillCount             = s.NpcKillCount;
        PvpKillCount             = s.PvpKillCount;
        TotalBountyCredits       = s.TotalBountyCredits;
        TotalCombatBondCredits   = s.TotalCombatBondCredits;
        TotalCapShipBondCredits  = s.TotalCapShipBondCredits;
        DeathCount               = s.DeathCount;
        InterdictionsAttempted   = s.InterdictionsAttempted;
        InterdictionsSucceeded   = s.InterdictionsSucceeded;
        TimesInterdicted         = s.TimesInterdicted;
        InterdictionsEscaped     = s.InterdictionsEscaped;
        HullLowWaterMark         = s.HullLowWaterMark;
        CrimeCount               = s.CrimeCount;

        // Rebuild top-factions list (top 5 by kill count)
        TopFactions.Clear();
        foreach (var kvp in s.KillsByFaction.OrderByDescending(k => k.Value).Take(5))
            TopFactions.Add(new FactionKillViewModel(kvp.Key, kvp.Value));
    }

    private void NotifyRankEstimatesChanged()
    {
        OnPropertyChanged(nameof(KillsEstimatePlus2));
        OnPropertyChanged(nameof(KillsEstimatePlus1));
        OnPropertyChanged(nameof(KillsEstimateEqual));
        OnPropertyChanged(nameof(KillsEstimateMinus1));
        OnPropertyChanged(nameof(KillsEstimateMinus2));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>Simple view model for a faction + kill count row.</summary>
public class FactionKillViewModel(string faction, int killCount)
{
    public string Faction   { get; } = faction;
    public int    KillCount { get; } = killCount;
}
