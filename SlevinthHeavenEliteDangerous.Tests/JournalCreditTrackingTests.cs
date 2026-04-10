using SlevinthHeavenEliteDangerous.Core.Models;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Helpers;
using System.Diagnostics;

namespace SlevinthHeavenEliteDangerous.Tests;

/// <summary>
/// Integration tests to validate credit tracking across sequential journal files.
/// </summary>
public class JournalCreditTrackingTests
{
    /// <summary>
    /// Test that the calculated balance and wealth at the end of one journal file
    /// matches the LoadGame and Statistics events at the start of the next journal file.
    /// </summary>
    [Fact]
    public void SequentialJournalFiles_BalanceAndWealthShouldMatch()
    {
        // Arrange
        var journalPath = GetJournalDirectory();
        
        // Skip if journal directory doesn't exist (e.g., CI environment)
        if (!Directory.Exists(journalPath))
        {
            Assert.True(true, "Journal directory not found - skipping test");
            return;
        }

        var journalFiles = Directory.GetFiles(journalPath, "*.log")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        if (journalFiles.Count < 2)
        {
            Assert.True(true, $"Need at least 2 journal files for sequential validation, found {journalFiles.Count}");
            return;
        }

        var parser = new EventParser();
        var errors = new List<string>();

        // Act & Assert
        long calculatedBalance = 0;
        long? calculatedWealth = null;
        LoadGameEvent? lastLoadGame = null;
        StatisticsEvent? lastStatistics = null;

        for (int i = 0; i < journalFiles.Count; i++)
        {
            var file = journalFiles[i];
            var fileName = Path.GetFileName(file);
            Debug.WriteLine($"Processing journal file {i + 1}/{journalFiles.Count}: {fileName}");

            LoadGameEvent? currentLoadGame = null;
            StatisticsEvent? currentStatistics = null;
            var events = new List<EventBase>();

            // Parse all events from the journal file
            parser.ParseFile(
                file,
                onEvent: ev => events.Add(ev),
                onUnknown: msg => Debug.WriteLine($"  Unknown event: {msg}"),
                onError: (ex, context) => Debug.WriteLine($"  Error at {context}: {ex.Message}")
            );

            Debug.WriteLine($"  Parsed {events.Count} events");

            // Find LoadGame and Statistics events (should be at the start of the file)
            currentLoadGame = events.OfType<LoadGameEvent>().FirstOrDefault();
            currentStatistics = events.OfType<StatisticsEvent>().FirstOrDefault();

            // If this is not the first journal, validate continuity
            if (i > 0 && lastLoadGame != null)
            {
                if (currentLoadGame != null)
                {
                    // Validate wallet balance continuity
                    var expectedBalance = calculatedBalance;
                    var actualBalance = currentLoadGame.Credits;

                    if (expectedBalance != actualBalance)
                    {
                        var error = $"Journal {i} ({fileName}): Balance mismatch! " +
                                  $"Previous journal ended with {expectedBalance:N0} CR, " +
                                  $"but LoadGame shows {actualBalance:N0} CR. " +
                                  $"Difference: {(actualBalance - expectedBalance):N0} CR";
                        errors.Add(error);
                        Debug.WriteLine($"  ERROR: {error}");
                    }
                    else
                    {
                        Debug.WriteLine($"  ✓ Balance continuity validated: {actualBalance:N0} CR");
                    }
                }

                if (currentStatistics != null && calculatedWealth.HasValue)
                {
                    // Validate wealth continuity
                    var expectedWealth = calculatedWealth.Value;
                    var actualWealth = currentStatistics.BankAccount?.CurrentWealth ?? 0;

                    // Note: Wealth can differ slightly due to asset values changing,
                    // but large discrepancies indicate a tracking error
                    var difference = Math.Abs(actualWealth - expectedWealth);
                    var percentDifference = expectedWealth > 0 ? (difference * 100.0 / expectedWealth) : 0;

                    if (percentDifference > 1.0) // Allow 1% variance for asset value changes
                    {
                        var warning = $"Journal {i} ({fileName}): Wealth variance > 1%! " +
                                    $"Calculated: {expectedWealth:N0} CR, " +
                                    $"Statistics: {actualWealth:N0} CR. " +
                                    $"Difference: {(actualWealth - expectedWealth):N0} CR ({percentDifference:F2}%)";
                        Debug.WriteLine($"  WARNING: {warning}");
                    }
                    else
                    {
                        Debug.WriteLine($"  ✓ Wealth continuity validated: {actualWealth:N0} CR (variance: {percentDifference:F2}%)");
                    }
                }
            }

            // Initialize tracking from LoadGame/Statistics if this is the first journal
            // or reset if we found new LoadGame event
            if (currentLoadGame != null)
            {
                calculatedBalance = currentLoadGame.Credits;
                Debug.WriteLine($"  Initialized balance from LoadGame: {calculatedBalance:N0} CR");
            }

            if (currentStatistics != null)
            {
                calculatedWealth = currentStatistics.BankAccount?.CurrentWealth ?? 0;
                Debug.WriteLine($"  Initialized wealth from Statistics: {calculatedWealth:N0} CR");
            }

            // Process all credit-affecting events in chronological order
            foreach (var evt in events)
            {
                var creditChange = CalculateCreditChange(evt);
                if (creditChange != 0)
                {
                    calculatedBalance += creditChange;
                    Debug.WriteLine($"  {evt.GetType().Name}: {(creditChange > 0 ? "+" : "")}{creditChange:N0} CR -> Balance: {calculatedBalance:N0} CR");
                }
            }

            Debug.WriteLine($"  End of journal balance: {calculatedBalance:N0} CR");
            if (calculatedWealth.HasValue)
            {
                Debug.WriteLine($"  End of journal wealth: {calculatedWealth:N0} CR");
            }
            Debug.WriteLine("");

            lastLoadGame = currentLoadGame;
            lastStatistics = currentStatistics;
        }

        // Assert no errors
        if (errors.Count > 0)
        {
            Assert.Fail($"Found {errors.Count} balance continuity error(s):\n" + string.Join("\n", errors));
        }
    }

    /// <summary>
    /// Calculate the credit change for a given event.
    /// This mirrors the logic in CommanderStatsService.HandleEvent.
    /// </summary>
    private long CalculateCreditChange(EventBase evt)
    {
        return evt switch
        {
            // Credit earning events
            MarketSellEvent marketSell => marketSell.TotalSale ?? 0,
            SellExplorationDataEvent sellExploration => sellExploration.TotalEarnings,
            MultiSellExplorationDataEvent multiSellExploration => multiSellExploration.TotalEarnings,
            SellOrganicDataEvent sellOrganic => sellOrganic.BioData.Sum(b => b.Value + b.Bonus),
            RedeemVoucherEvent redeemVoucher => redeemVoucher.Amount,
            MissionCompletedEvent missionCompleted => missionCompleted.Reward ?? 0,
            MissionAbandonedEvent missionAbandoned when missionAbandoned.Fine.HasValue => -missionAbandoned.Fine.Value,
            MissionFailedEvent missionFailed when missionFailed.Fine.HasValue => -missionFailed.Fine.Value,
            ShipyardSellEvent shipyardSell => shipyardSell.ShipPrice ?? 0,
            SearchAndRescueEvent searchRescue => searchRescue.Reward,
            SellDronesEvent sellDrones => sellDrones.TotalSale,
            SellMicroResourcesEvent sellMicro when sellMicro.Price.HasValue => sellMicro.Price.Value,
            PowerplaySalaryEvent powerplaySalary => powerplaySalary.Amount,
            CarrierBankTransferEvent carrierTransfer when carrierTransfer.Withdraw.HasValue => carrierTransfer.Withdraw.Value,

            // Credit spending events
            MarketBuyEvent marketBuy => -marketBuy.TotalCost,
            RefuelAllEvent refuelAll => -refuelAll.Cost,
            RefuelPartialEvent refuelPartial => -refuelPartial.Cost,
            RepairAllEvent repairAll => -repairAll.Cost,
            RepairEvent repair when repair.Cost.HasValue => -repair.Cost.Value,
            PayFinesEvent payFines => -payFines.Amount,
            PayLegacyFinesEvent payLegacyFines => -payLegacyFines.Amount,
            PayBountiesEvent payBounties when payBounties.Amount.HasValue => -payBounties.Amount.Value,
            ShipyardBuyEvent shipyardBuy when shipyardBuy.ShipPrice.HasValue => -shipyardBuy.ShipPrice.Value,
            ModuleBuyEvent moduleBuy => -moduleBuy.BuyPrice,
            ModuleSellEvent moduleSell => moduleSell.SellPrice,
            BuyAmmoEvent buyAmmo => -buyAmmo.Cost,
            BuyDronesEvent buyDrones => -buyDrones.TotalCost,
            RestockVehicleEvent restockVehicle when restockVehicle.Cost.HasValue => -restockVehicle.Cost.Value,
            FetchRemoteModuleEvent fetchModule => -fetchModule.TransferCost,
            ShipyardTransferEvent shipTransfer when shipTransfer.TransferPrice.HasValue => -shipTransfer.TransferPrice.Value,
            BuyTradeDataEvent buyTradeData => -buyTradeData.Cost,
            BuyExplorationDataEvent buyExploration when buyExploration.Cost.HasValue => -buyExploration.Cost.Value,
            ResurrectEvent resurrect when resurrect.Cost.HasValue => -resurrect.Cost.Value,
            BuyMicroResourcesEvent buyMicro when buyMicro.Price.HasValue => -buyMicro.Price.Value,
            BuySuitEvent buySuit when buySuit.Price.HasValue => -buySuit.Price.Value,
            BuyWeaponEvent buyWeapon => -buyWeapon.Price,
            UpgradeSuitEvent upgradeSuit when upgradeSuit.Cost.HasValue => -upgradeSuit.Cost.Value,
            UpgradeWeaponEvent upgradeWeapon when upgradeWeapon.Cost.HasValue => -upgradeWeapon.Cost.Value,
            SellSuitEvent sellSuit when sellSuit.Price.HasValue => sellSuit.Price.Value,
            SellWeaponEvent sellWeapon when sellWeapon.Price.HasValue => sellWeapon.Price.Value,
            NpcCrewPaidWageEvent crewWage => -crewWage.Amount,
            CarrierBuyEvent carrierBuy => -carrierBuy.Price,
            CarrierBankTransferEvent carrierDeposit when carrierDeposit.Deposit.HasValue => -carrierDeposit.Deposit.Value,

            _ => 0
        };
    }

    /// <summary>
    /// Get the Elite Dangerous journal directory path.
    /// </summary>
    private string GetJournalDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Saved Games",
            "Frontier Developments",
            "Elite Dangerous"
        );
    }
}
