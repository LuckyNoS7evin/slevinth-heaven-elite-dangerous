using SlevinthHeavenEliteDangerous.Core.Models;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Helpers;
using System.Diagnostics;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Tests;

/// <summary>
/// Diagnostic tests to identify missing credit events in journals with large discrepancies.
/// </summary>
public class JournalDiagnosticTests
{
    /// <summary>
    /// Analyze specific journal files to find untracked credit events.
    /// Run this test to investigate discrepancies found by SequentialJournalFiles_BalanceAndWealthShouldMatch.
    /// </summary>
    [Fact]
    public void AnalyzeJournalsWithLargeDiscrepancies()
    {
        var journalPath = GetJournalDirectory();

        if (!Directory.Exists(journalPath))
        {
            Assert.True(true, "Journal directory not found - skipping test");
            return;
        }

        var journalFiles = Directory.GetFiles(journalPath, "*.log")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        if (journalFiles.Count == 0)
        {
            Assert.True(true, "No journal files found");
            return;
        }

        // Target journals with large discrepancies (from test results)
        var targetJournals = new[]
        {
            "Journal.2026-02-21T102143.01.log", // Journal 20: -41M CR
            "Journal.2026-03-04T172930.01.log", // Journal 35: -49M CR
            "Journal.2026-02-25T175718.01.log", // Journal 29: -32M CR
            "Journal.2026-04-06T144830.01.log", // Journal 56: -11M CR
            "Journal.2026-04-07T164157.01.log", // Journal 57: -17M CR
            "Journal.2026-04-10T100037.01.log"  // Journal 62: -10M CR
        };

        var parser = new EventParser();
        var findings = new List<string>();
        var output = new System.Text.StringBuilder();

        void Log(string message)
        {
            output.AppendLine(message);
            Debug.WriteLine(message);
        }

        foreach (var targetJournal in targetJournals)
        {
            var journalFile = journalFiles.FirstOrDefault(f => Path.GetFileName(f) == targetJournal);
            if (journalFile == null)
                continue;

            Log($"\n{new string('=', 80)}");
            Log($"ANALYZING: {targetJournal}");
            Log($"{new string('=', 80)}\n");

            var allEvents = new List<EventBase>();
            var unknownCreditEvents = new List<(string eventName, string json)>();

            // Parse all events and raw JSON
            var lines = File.ReadAllLines(journalFile);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Try to parse as event
                if (parser.TryParseLine(line, out var evt, out var eventName, out var error, out var serializationFailure, journalFile))
                {
                    if (evt != null)
                        allEvents.Add(evt);
                }

                // Check if line contains credit-related properties
                if (ContainsCreditProperties(line))
                {
                    var doc = JsonDocument.Parse(line);
                    var jsonEventName = doc.RootElement.GetProperty("event").GetString() ?? "Unknown";

                    // Check if this event type is tracked
                    if (!IsTrackedEvent(evt))
                    {
                        unknownCreditEvents.Add((jsonEventName, line));
                    }
                }
            }

            // Calculate tracked vs total
            var trackedCredits = allEvents.Sum(e => CalculateCreditChange(e));

            Log($"Total events: {allEvents.Count}");
            Log($"Tracked credit change: {trackedCredits:N0} CR\n");

            if (unknownCreditEvents.Count > 0)
            {
                Log($"Found {unknownCreditEvents.Count} potentially untracked credit events:\n");

                var groupedEvents = unknownCreditEvents
                    .GroupBy(e => e.eventName)
                    .OrderByDescending(g => g.Count());

                foreach (var group in groupedEvents)
                {
                    Log($"  {group.Key} ({group.Count()} occurrences):");

                    // Show first example
                    var example = group.First().json;
                    var doc = JsonDocument.Parse(example);
                    var creditProps = ExtractCreditProperties(doc.RootElement);

                    Log($"    Example properties: {string.Join(", ", creditProps)}");
                    Log($"    Full JSON: {example}\n");

                    findings.Add($"{targetJournal}: {group.Key} ({group.Count()}x) - {string.Join(", ", creditProps)}");
                }
            }
            else
            {
                Log("No untracked credit events found. Discrepancy may be due to:");
                Log("  - Events occurring between journal files");
                Log("  - Property name differences (check actual event class properties)");
                Log("  - Calculation logic differences\n");
            }
        }

        // Output summary
        Log($"\n{new string('=', 80)}");
        Log("SUMMARY OF FINDINGS");
        Log($"{new string('=', 80)}\n");

        if (findings.Count > 0)
        {
            foreach (var finding in findings)
            {
                Log($"  • {finding}");
            }

            Log($"\n\nRecommendation: Review these event types and add handlers to CommanderStatsService.cs");
        }
        else
        {
            Log("No obviously untracked events found in target journals.");
            Log("Discrepancies may be due to:");
            Log("  - Events in adjacent journals (before/after)");
            Log("  - Incorrect property mappings in existing handlers");
            Log("  - Game balance changes between sessions");
        }

        // Write output to file
        var outputPath = Path.Combine(Path.GetTempPath(), "JournalDiagnosticResults.txt");
        File.WriteAllText(outputPath, output.ToString());
        Log($"\n\nFull results written to: {outputPath}");

        // Also output to test result
        Assert.True(true, $"Analysis complete. Results written to: {outputPath}\n\n{output}");
    }

    /// <summary>
    /// Check if a JSON line contains credit-related properties.
    /// </summary>
    private bool ContainsCreditProperties(string json)
    {
        var creditKeywords = new[] 
        { 
            "\"Cost\":", "\"Price\":", "\"Reward\":", "\"Amount\":", 
            "\"TotalCost\":", "\"TotalSale\":", "\"TotalEarnings\":",
            "\"BuyPrice\":", "\"SellPrice\":", "\"ShipPrice\":",
            "\"Credits\":", "\"Balance\":", "\"TransferCost\":",
            "\"TransferPrice\":", "\"Deposit\":", "\"Withdraw\":",
            "\"Value\":", "\"Bonus\":"
        };
        
        return creditKeywords.Any(keyword => json.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Extract credit-related properties from a JSON element.
    /// </summary>
    private List<string> ExtractCreditProperties(JsonElement element)
    {
        var props = new List<string>();
        
        foreach (var prop in element.EnumerateObject())
        {
            var name = prop.Name;
            var lowerName = name.ToLower();
            
            if (lowerName.Contains("cost") || lowerName.Contains("price") || 
                lowerName.Contains("reward") || lowerName.Contains("amount") ||
                lowerName.Contains("sale") || lowerName.Contains("earnings") ||
                lowerName.Contains("credits") || lowerName.Contains("balance") ||
                lowerName.Contains("deposit") || lowerName.Contains("withdraw"))
            {
                var value = prop.Value.ValueKind == JsonValueKind.Number 
                    ? prop.Value.GetInt64().ToString("N0") 
                    : prop.Value.ToString();
                props.Add($"{name}={value}");
            }
        }
        
        return props;
    }

    /// <summary>
    /// Check if an event type is currently being tracked.
    /// </summary>
    private bool IsTrackedEvent(EventBase? evt)
    {
        if (evt == null)
            return false;

        return evt is MarketSellEvent or
               SellExplorationDataEvent or
               MultiSellExplorationDataEvent or
               SellOrganicDataEvent or
               RedeemVoucherEvent or
               MissionCompletedEvent or
               ShipyardSellEvent or
               SearchAndRescueEvent or
               SellDronesEvent or
               SellMicroResourcesEvent or
               PowerplaySalaryEvent or
               CarrierBankTransferEvent or
               MarketBuyEvent or
               RefuelAllEvent or
               RefuelPartialEvent or
               RepairAllEvent or
               RepairEvent or
               PayFinesEvent or
               PayLegacyFinesEvent or
               PayBountiesEvent or
               ShipyardBuyEvent or
               ModuleBuyEvent or
               ModuleSellEvent or
               BuyAmmoEvent or
               BuyDronesEvent or
               RestockVehicleEvent or
               FetchRemoteModuleEvent or
               ShipyardTransferEvent or
               BuyTradeDataEvent or
               BuyExplorationDataEvent or
               ResurrectEvent or
               BuyMicroResourcesEvent or
               BuySuitEvent or
               BuyWeaponEvent or
               UpgradeSuitEvent or
               UpgradeWeaponEvent or
               SellSuitEvent or
               SellWeaponEvent or
               NpcCrewPaidWageEvent or
               CarrierBuyEvent;
    }

    /// <summary>
    /// Calculate credit change for a given event (same as main test).
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
