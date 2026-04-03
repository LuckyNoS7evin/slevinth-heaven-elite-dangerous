using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Services.Models;
using System;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Handles the Statistics event, persists the latest snapshot, and notifies the UI.
/// The Statistics event fires on every game load with fully up-to-date values,
/// so we simply replace the persisted snapshot each time.
/// </summary>
public sealed class CommanderStatsService : IEventHandler
{
    private readonly CommanderStatsDataService _dataService = new();
    private CommanderStatsModel _stats = new();

    public event EventHandler<CommanderStatsUpdatedEventArgs>? StatsUpdated;

    public void HandleEvent(EventBase evt)
    {
        if (evt is StatisticsEvent statistics)
            HandleStatisticsEvent(statistics);
        else if (evt is LoadGameEvent load)
            HandleLoadGameEvent(load);
    }

    public async Task LoadDataAsync()
    {
        var data = await _dataService.LoadDataAsync();
        if (data != null)
        {
            _stats = data;
            StatsUpdated?.Invoke(this, new CommanderStatsUpdatedEventArgs(_stats));
        }
    }

    public CommanderStatsModel GetStats() => _stats;

    private void HandleStatisticsEvent(StatisticsEvent evt)
    {
        _stats = new CommanderStatsModel
        {
            // Bank Account
            CurrentWealth      = evt.BankAccount?.CurrentWealth ?? 0,
            OwnedShipCount     = evt.BankAccount?.OwnedShipCount ?? 0,

            // Exploration
            SystemsVisited          = evt.Exploration?.SystemsVisited ?? 0,
            ExplorationProfits      = evt.Exploration?.ExplorationProfits ?? 0,
            TotalHyperspaceJumps    = evt.Exploration?.TotalHyperspaceJumps ?? 0,
            TotalHyperspaceDistance = evt.Exploration?.TotalHyperspaceDistance ?? 0,
            TimePlayed              = evt.Exploration?.TimePlayed ?? 0,

            // Trading
            MarketProfits     = evt.Trading?.MarketProfits ?? 0,
            MarketsTradedWith = evt.Trading?.MarketsTradedWith ?? 0,

            // Combat
            BountiesClaimed    = evt.Combat?.BountiesClaimed ?? 0,
            BountyHuntingProfit = evt.Combat?.BountyHuntingProfit ?? 0,

            // Mining
            MiningProfits = evt.Mining?.MiningProfits ?? 0,

            // Exobiology
            ExobiologyProfits    = evt.Exobiology?.ExobiologyProfits ?? 0,
            OrganicSpeciesAnalysed = evt.Exobiology?.OrganicData ?? 0,
        };

        StatsUpdated?.Invoke(this, new CommanderStatsUpdatedEventArgs(_stats));
        _ = _dataService.SaveDataAsync(_stats);
    }

    private void HandleLoadGameEvent(LoadGameEvent evt)
    {
        // LoadGameEvent contains the current bank Credits when the game is started/loaded.
        // Use it to set the commander bank balance so the UI can show the in-game wallet value.
        _stats.WalletBalance = evt.Credits;

        StatsUpdated?.Invoke(this, new CommanderStatsUpdatedEventArgs(_stats));
        _ = _dataService.SaveDataAsync(_stats);
    }
}

public class CommanderStatsUpdatedEventArgs : EventArgs
{
    public CommanderStatsModel Stats { get; }
    public CommanderStatsUpdatedEventArgs(CommanderStatsModel stats) => Stats = stats;
}
