using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Services.Models;
using System;
using System.Linq;
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
        // Credit earning events
        else if (evt is MarketSellEvent marketSell)
            HandleCreditChange(marketSell.TotalSale ?? 0, "Market Sale");
        else if (evt is SellExplorationDataEvent sellExploration)
            HandleCreditChange(sellExploration.TotalEarnings, "Exploration Data");
        else if (evt is MultiSellExplorationDataEvent multiSellExploration)
            HandleCreditChange(multiSellExploration.TotalEarnings, "Exploration Data");
        else if (evt is SellOrganicDataEvent sellOrganic)
            HandleCreditChange(sellOrganic.BioData.Sum(b => b.Value + b.Bonus), "Organic Data");
        else if (evt is RedeemVoucherEvent redeemVoucher)
            HandleCreditChange(redeemVoucher.Amount, "Voucher");
        else if (evt is MissionCompletedEvent missionCompleted)
            HandleCreditChange(missionCompleted.Reward ?? 0, "Mission Reward");
        else if (evt is MissionAbandonedEvent missionAbandoned && missionAbandoned.Fine.HasValue)
            HandleCreditChange(-missionAbandoned.Fine.Value, "Mission Abandoned Fine");
        else if (evt is MissionFailedEvent missionFailed && missionFailed.Fine.HasValue)
            HandleCreditChange(-missionFailed.Fine.Value, "Mission Failed Fine");
        else if (evt is ShipyardSellEvent shipyardSell)
            HandleCreditChange(shipyardSell.ShipPrice ?? 0, "Ship Sale");
        else if (evt is SearchAndRescueEvent searchRescue)
            HandleCreditChange(searchRescue.Reward, "Search & Rescue");
        else if (evt is SellDronesEvent sellDrones)
            HandleCreditChange(sellDrones.TotalSale, "Drone Sale");
        else if (evt is SellMicroResourcesEvent sellMicro && sellMicro.Price.HasValue)
            HandleCreditChange(sellMicro.Price.Value, "Micro Resources");
        else if (evt is PowerplaySalaryEvent powerplaySalary)
            HandleCreditChange(powerplaySalary.Amount, "Powerplay Salary");
        else if (evt is CarrierBankTransferEvent carrierTransfer)
        {
            // Withdraw adds to wallet, Deposit removes from wallet
            if (carrierTransfer.Withdraw.HasValue)
                HandleCreditChange(carrierTransfer.Withdraw.Value, "Carrier Withdrawal");
            else if (carrierTransfer.Deposit.HasValue)
                HandleCreditChange(-carrierTransfer.Deposit.Value, "Carrier Deposit");
        }
        // Credit spending events
        else if (evt is MarketBuyEvent marketBuy)
            HandleCreditChange(-marketBuy.TotalCost, "Market Purchase");
        else if (evt is RefuelAllEvent refuelAll)
            HandleCreditChange(-refuelAll.Cost, "Refuel");
        else if (evt is RefuelPartialEvent refuelPartial)
            HandleCreditChange(-refuelPartial.Cost, "Refuel");
        else if (evt is RepairAllEvent repairAll)
            HandleCreditChange(-repairAll.Cost, "Repair");
        else if (evt is RepairEvent repair && repair.Cost.HasValue)
            HandleCreditChange(-repair.Cost.Value, "Repair");
        else if (evt is PayFinesEvent payFines)
            HandleCreditChange(-payFines.Amount, "Fines");
        else if (evt is PayLegacyFinesEvent payLegacyFines)
            HandleCreditChange(-payLegacyFines.Amount, "Legacy Fines");
        else if (evt is PayBountiesEvent payBounties && payBounties.Amount.HasValue)
            HandleCreditChange(-payBounties.Amount.Value, "Bounties");
        else if (evt is ShipyardBuyEvent shipyardBuy && shipyardBuy.ShipPrice.HasValue)
            HandleCreditChange(-shipyardBuy.ShipPrice.Value, "Ship Purchase");
        else if (evt is ModuleBuyEvent moduleBuy)
            HandleCreditChange(-moduleBuy.BuyPrice, "Module Purchase");
        else if (evt is ModuleSellEvent moduleSell)
            HandleCreditChange(moduleSell.SellPrice, "Module Sale");
        else if (evt is BuyAmmoEvent buyAmmo)
            HandleCreditChange(-buyAmmo.Cost, "Ammo");
        else if (evt is BuyDronesEvent buyDrones)
            HandleCreditChange(-buyDrones.TotalCost, "Drones");
        else if (evt is RestockVehicleEvent restockVehicle && restockVehicle.Cost.HasValue)
            HandleCreditChange(-restockVehicle.Cost.Value, "Vehicle Restock");
        else if (evt is FetchRemoteModuleEvent fetchModule)
            HandleCreditChange(-fetchModule.TransferCost, "Module Transfer");
        else if (evt is ShipyardTransferEvent shipTransfer && shipTransfer.TransferPrice.HasValue)
            HandleCreditChange(-shipTransfer.TransferPrice.Value, "Ship Transfer");
        else if (evt is BuyTradeDataEvent buyTradeData)
            HandleCreditChange(-buyTradeData.Cost, "Trade Data");
        else if (evt is BuyExplorationDataEvent buyExploration && buyExploration.Cost.HasValue)
            HandleCreditChange(-buyExploration.Cost.Value, "Exploration Data");
        else if (evt is ResurrectEvent resurrect && resurrect.Cost.HasValue)
            HandleCreditChange(-resurrect.Cost.Value, "Resurrection");
        else if (evt is BuyMicroResourcesEvent buyMicro && buyMicro.Price.HasValue)
            HandleCreditChange(-buyMicro.Price.Value, "Micro Resources");
        else if (evt is BuySuitEvent buySuit && buySuit.Price.HasValue)
            HandleCreditChange(-buySuit.Price.Value, "Suit Purchase");
        else if (evt is BuyWeaponEvent buyWeapon)
            HandleCreditChange(-buyWeapon.Price, "Weapon Purchase");
        else if (evt is UpgradeSuitEvent upgradeSuit && upgradeSuit.Cost.HasValue)
            HandleCreditChange(-upgradeSuit.Cost.Value, "Suit Upgrade");
        else if (evt is UpgradeWeaponEvent upgradeWeapon && upgradeWeapon.Cost.HasValue)
            HandleCreditChange(-upgradeWeapon.Cost.Value, "Weapon Upgrade");
        else if (evt is SellSuitEvent sellSuit && sellSuit.Price.HasValue)
            HandleCreditChange(sellSuit.Price.Value, "Suit Sale");
        else if (evt is SellWeaponEvent sellWeapon && sellWeapon.Price.HasValue)
            HandleCreditChange(sellWeapon.Price.Value, "Weapon Sale");
        else if (evt is NpcCrewPaidWageEvent crewWage)
            HandleCreditChange(-crewWage.Amount, "NPC Crew Wage");
        else if (evt is CarrierBuyEvent carrierBuy)
            HandleCreditChange(-carrierBuy.Price, "Fleet Carrier Purchase");
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
        var previousWalletBalance = _stats.WalletBalance;

        _stats = new CommanderStatsModel
        {
            // Bank Account
            CurrentWealth      = evt.BankAccount?.CurrentWealth ?? 0,
            WalletBalance      = previousWalletBalance,
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

    private void HandleCreditChange(long amount, string source)
    {
        // Update the wallet balance by the transaction amount
        _stats.WalletBalance += amount;

        StatsUpdated?.Invoke(this, new CommanderStatsUpdatedEventArgs(_stats));
        _ = _dataService.SaveDataAsync(_stats);
    }
}

public class CommanderStatsUpdatedEventArgs : EventArgs
{
    public CommanderStatsModel Stats { get; }
    public CommanderStatsUpdatedEventArgs(CommanderStatsModel stats) => Stats = stats;
}
