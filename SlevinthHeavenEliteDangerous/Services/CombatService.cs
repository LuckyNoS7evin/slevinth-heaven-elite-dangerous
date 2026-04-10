using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Service for tracking combat events: kills, bounties, crimes, deaths, interdictions, hull damage.
/// </summary>
public sealed class CombatService : IEventHandler, IDisposable
{
    private const int MaxKillLogEntries = 500;

    private readonly CombatDataService _dataService = new();

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
    private readonly Dictionary<string, int> _killsByFaction = [];
    private readonly List<CombatKillRecord> _killLog = [];

    // Track recent targets for rank correlation in wing/squad scenarios
    private const int MaxRecentTargets = 20;
    private readonly List<RecentTargetInfo> _recentTargets = [];

    private bool _isLoading;
    private DateTime _lastSaveRequest = DateTime.MinValue;
    private readonly TimeSpan _saveDebounceDelay = TimeSpan.FromMilliseconds(500);
    private Task? _saveTask;
    private readonly object _saveLock = new();

    // ── Events ──────────────────────────────────────────────────────────────

    public event EventHandler<CombatKillEventArgs>? KillLogged;
    public event EventHandler<CombatStatsChangedEventArgs>? StatsChanged;
    public event EventHandler<CombatDataLoadedEventArgs>? DataLoaded;

    // ── IEventHandler ────────────────────────────────────────────────────────

    public void HandleEvent(EventBase evt)
    {
        switch (evt)
        {
            case BountyEvent bounty:            HandleBounty(bounty);           break;
            case CapShipBondEvent capShip:      HandleCapShipBond(capShip);     break;
            case FactionKillBondEvent bond:     HandleFactionKillBond(bond);    break;
            case PVPKillEvent pvp:              HandlePvpKill(pvp);             break;
            case CommitCrimeEvent crime:        HandleCommitCrime(crime);       break;
            case DiedEvent died:                HandleDied(died);               break;
            case InterdictionEvent interdict:   HandleInterdiction(interdict);  break;
            case InterdictedEvent interdicted:  HandleInterdicted(interdicted); break;
            case EscapeInterdictionEvent esc:   HandleEscapeInterdiction(esc);  break;
            case HullDamageEvent hull:          HandleHullDamage(hull);         break;
            case ShipTargetedEvent targeted:    HandleShipTargeted(targeted);   break;
        }
    }

    // ── Load / Save ──────────────────────────────────────────────────────────

    public async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            var data = await _dataService.LoadDataAsync();
            if (data == null)
            {
                DataLoaded?.Invoke(this, new CombatDataLoadedEventArgs(BuildSnapshot()));
                return;
            }

            _npcKillCount              = data.NpcKillCount;
            _pvpKillCount              = data.PvpKillCount;
            _totalBountyCredits        = data.TotalBountyCredits;
            _totalCombatBondCredits    = data.TotalCombatBondCredits;
            _totalCapShipBondCredits   = data.TotalCapShipBondCredits;
            _deathCount                = data.DeathCount;
            _interdictionsAttempted    = data.InterdictionsAttempted;
            _interdictionsSucceeded    = data.InterdictionsSucceeded;
            _timesInterdicted          = data.TimesInterdicted;
            _interdictionsEscaped      = data.InterdictionsEscaped;
            _hullLowWaterMark          = data.HullLowWaterMark;
            _crimeCount                = data.CrimeCount;

            _killsByFaction.Clear();
            foreach (var kvp in data.KillsByFaction)
                _killsByFaction[kvp.Key] = kvp.Value;

            _killLog.Clear();
            foreach (var entry in data.KillLog)
                _killLog.Add(entry);

            DataLoaded?.Invoke(this, new CombatDataLoadedEventArgs(BuildSnapshot()));
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
            DateTime lastRequest;
            lock (_saveLock) { lastRequest = _lastSaveRequest; }

            var remaining = _saveDebounceDelay - (DateTime.UtcNow - lastRequest);
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

        try
        {
            if (_isLoading) return;
            await _dataService.SaveDataAsync(BuildSnapshot());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CombatService] Error saving: {ex.Message}");
        }
    }

    private CombatStateModel BuildSnapshot() => new()
    {
        NpcKillCount            = _npcKillCount,
        PvpKillCount            = _pvpKillCount,
        TotalBountyCredits      = _totalBountyCredits,
        TotalCombatBondCredits  = _totalCombatBondCredits,
        TotalCapShipBondCredits = _totalCapShipBondCredits,
        DeathCount              = _deathCount,
        InterdictionsAttempted  = _interdictionsAttempted,
        InterdictionsSucceeded  = _interdictionsSucceeded,
        TimesInterdicted        = _timesInterdicted,
        InterdictionsEscaped    = _interdictionsEscaped,
        HullLowWaterMark        = _hullLowWaterMark,
        CrimeCount              = _crimeCount,
        KillsByFaction          = new Dictionary<string, int>(_killsByFaction),
        KillLog                 = [.. _killLog],
    };

    // ── Event handlers ───────────────────────────────────────────────────────

    private void HandleBounty(BountyEvent evt)
    {
        _npcKillCount++;
        _totalBountyCredits += evt.TotalReward ?? 0;

        if (!string.IsNullOrEmpty(evt.VictimFaction))
            IncrementFactionKills(evt.VictimFaction);

        // Try to find matching target from recent targets
        var matchedTarget = FindMatchingTarget(evt);

        var record = new CombatKillRecord
        {
            Timestamp        = evt.Timestamp,
            Target           = evt.Target_Localised.Length > 0 ? evt.Target_Localised : evt.Target,
            VictimFaction    = evt.VictimFaction,
            IsPvp            = false,
            CreditsEarned    = evt.TotalReward ?? 0,
            KillType         = "Bounty",
            PilotName        = matchedTarget?.PilotName,
            VictimPilotRank  = matchedTarget?.PilotRank,
        };

        AddKillRecord(record);
        StatsChanged?.Invoke(this, new CombatStatsChangedEventArgs(BuildSnapshot()));
        ScheduleSave();
    }

    private void HandleCapShipBond(CapShipBondEvent evt)
    {
        _totalCapShipBondCredits += evt.Reward;

        if (!string.IsNullOrEmpty(evt.VictimFaction))
            IncrementFactionKills(evt.VictimFaction);

        var record = new CombatKillRecord
        {
            Timestamp     = evt.Timestamp,
            Target        = evt.VictimFaction,
            VictimFaction = evt.VictimFaction,
            IsPvp         = false,
            CreditsEarned = evt.Reward,
            KillType      = "CapShipBond",
        };

        AddKillRecord(record);
        StatsChanged?.Invoke(this, new CombatStatsChangedEventArgs(BuildSnapshot()));
        ScheduleSave();
    }

    private void HandleFactionKillBond(FactionKillBondEvent evt)
    {
        _totalCombatBondCredits += evt.Reward;

        if (!string.IsNullOrEmpty(evt.VictimFaction))
            IncrementFactionKills(evt.VictimFaction);

        var record = new CombatKillRecord
        {
            Timestamp     = evt.Timestamp,
            Target        = evt.VictimFaction,
            VictimFaction = evt.VictimFaction,
            IsPvp         = false,
            CreditsEarned = evt.Reward,
            KillType      = "CombatBond",
        };

        AddKillRecord(record);
        StatsChanged?.Invoke(this, new CombatStatsChangedEventArgs(BuildSnapshot()));
        ScheduleSave();
    }

    private void HandlePvpKill(PVPKillEvent evt)
    {
        _pvpKillCount++;

        var record = new CombatKillRecord
        {
            Timestamp        = evt.Timestamp,
            Target           = evt.Victim,
            IsPvp            = true,
            VictimCombatRank = evt.CombatRank,
            KillType         = "PVP",
        };

        AddKillRecord(record);
        StatsChanged?.Invoke(this, new CombatStatsChangedEventArgs(BuildSnapshot()));
        ScheduleSave();
    }

    private void HandleCommitCrime(CommitCrimeEvent evt)
    {
        _crimeCount++;
        StatsChanged?.Invoke(this, new CombatStatsChangedEventArgs(BuildSnapshot()));
        ScheduleSave();
    }

    private void HandleDied(DiedEvent evt)
    {
        _deathCount++;
        // Reset hull low-water mark after death — starts fresh each life
        _hullLowWaterMark = 1.0;
        StatsChanged?.Invoke(this, new CombatStatsChangedEventArgs(BuildSnapshot()));
        ScheduleSave();
    }

    private void HandleInterdiction(InterdictionEvent evt)
    {
        _interdictionsAttempted++;
        if (evt.Success) _interdictionsSucceeded++;
        StatsChanged?.Invoke(this, new CombatStatsChangedEventArgs(BuildSnapshot()));
        ScheduleSave();
    }

    private void HandleInterdicted(InterdictedEvent evt)
    {
        _timesInterdicted++;
        StatsChanged?.Invoke(this, new CombatStatsChangedEventArgs(BuildSnapshot()));
        ScheduleSave();
    }

    private void HandleEscapeInterdiction(EscapeInterdictionEvent evt)
    {
        _interdictionsEscaped++;
        StatsChanged?.Invoke(this, new CombatStatsChangedEventArgs(BuildSnapshot()));
        ScheduleSave();
    }

    private void HandleHullDamage(HullDamageEvent evt)
    {
        // Only track player ship hull, not NPC or fighter
        if (evt.PlayerPilot != true) return;
        if (evt.Fighter == true) return;

        double health = evt.Health ?? 1.0;
        if (health < _hullLowWaterMark)
        {
            _hullLowWaterMark = health;
            StatsChanged?.Invoke(this, new CombatStatsChangedEventArgs(BuildSnapshot()));
            ScheduleSave();
        }
    }

    private void HandleShipTargeted(ShipTargetedEvent evt)
    {
        // Store recent target info for correlation with future bounty events
        if (evt.TargetLocked && !string.IsNullOrEmpty(evt.Ship))
        {
            var targetInfo = new RecentTargetInfo
            {
                Timestamp = evt.Timestamp,
                PilotName = !string.IsNullOrEmpty(evt.PilotName_Localised) 
                    ? evt.PilotName_Localised 
                    : evt.PilotName,
                PilotRank = evt.PilotRank,
                Ship = !string.IsNullOrEmpty(evt.Ship_Localised) 
                    ? evt.Ship_Localised 
                    : evt.Ship,
                Faction = evt.Faction,
                Bounty = evt.Bounty ?? 0,
            };

            _recentTargets.Insert(0, targetInfo);

            // Keep buffer at max size
            if (_recentTargets.Count > MaxRecentTargets)
                _recentTargets.RemoveRange(MaxRecentTargets, _recentTargets.Count - MaxRecentTargets);
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private RecentTargetInfo? FindMatchingTarget(BountyEvent evt)
    {
        // Match recent targets by faction, ship type, and bounty amount
        // Bounty received might be partial in wing/squad, so check if it's <= potential bounty
        var targetShip = evt.Target_Localised.Length > 0 ? evt.Target_Localised : evt.Target;
        var bountyReceived = evt.TotalReward ?? 0;

        // Look through recent targets (most recent first)
        foreach (var target in _recentTargets)
        {
            // Check if this target matches the bounty event
            bool factionMatch = string.Equals(target.Faction, evt.VictimFaction, StringComparison.OrdinalIgnoreCase);
            bool shipMatch = string.Equals(target.Ship, targetShip, StringComparison.OrdinalIgnoreCase);
            bool bountyValid = target.Bounty == 0 || bountyReceived <= target.Bounty;
            bool recentEnough = (evt.Timestamp - target.Timestamp).TotalSeconds < 30; // Within 30 seconds

            if (factionMatch && shipMatch && bountyValid && recentEnough)
            {
                return target;
            }
        }

        return null; // No matching target found
    }

    private void IncrementFactionKills(string faction)
    {
        _killsByFaction.TryGetValue(faction, out int current);
        _killsByFaction[faction] = current + 1;
    }

    private void AddKillRecord(CombatKillRecord record)
    {
        // Check for duplicates within the last 5 seconds with same target and kill type
        var isDuplicate = _killLog.Any(existing =>
            Math.Abs((existing.Timestamp - record.Timestamp).TotalSeconds) < 5 &&
            existing.Target == record.Target &&
            existing.KillType == record.KillType &&
            existing.CreditsEarned == record.CreditsEarned);

        if (isDuplicate)
        {
            System.Diagnostics.Debug.WriteLine($"[CombatService] Duplicate kill log entry detected and skipped: {record.Target} at {record.Timestamp:HH:mm:ss}");
            return;
        }

        _killLog.Insert(0, record);

        if (_killLog.Count > MaxKillLogEntries)
            _killLog.RemoveRange(MaxKillLogEntries, _killLog.Count - MaxKillLogEntries);

        KillLogged?.Invoke(this, new CombatKillEventArgs(record));
    }

    public void Dispose()
    {
        if (_dataService is IDisposable d) d.Dispose();
    }
}

// ── Helper classes ────────────────────────────────────────────────────────────

/// <summary>
/// Tracks recent ship targets for correlation with bounty/bond events
/// </summary>
internal class RecentTargetInfo
{
    public DateTime Timestamp { get; set; }
    public string PilotName { get; set; } = string.Empty;
    public string PilotRank { get; set; } = string.Empty;
    public string Ship { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;
    public int Bounty { get; set; }
}

// ── Event args ────────────────────────────────────────────────────────────────

public class CombatKillEventArgs(CombatKillRecord record) : EventArgs
{
    public CombatKillRecord Record { get; } = record;
}

public class CombatStatsChangedEventArgs(CombatStateModel state) : EventArgs
{
    public CombatStateModel State { get; } = state;
}

public class CombatDataLoadedEventArgs(CombatStateModel state) : EventArgs
{
    public CombatStateModel State { get; } = state;
}


