using SlevinthHeavenEliteDangerous.Core.Models;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Helpers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Tests;

/// <summary>
/// Tests to identify spending events that we're not tracking.
/// Focuses on journals with NEGATIVE discrepancies (we're tracking too much income).
/// </summary>
public class MissingSpendingEventsTests
{
    [Fact]
    public void FindMissingSpendingEvents()
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

        if (journalFiles.Count < 2)
        {
            Assert.True(true, "Need at least 2 journal files");
            return;
        }

        var output = new StringBuilder();
        void Log(string message)
        {
            output.AppendLine(message);
            Debug.WriteLine(message);
        }

        var parser = new EventParser();

        // Track journals with significant negative discrepancies
        var journalsToAnalyze = new[]
        {
            ("Journal.2026-03-04T172930.01.log", -49_622_305),  // -49M CR
            ("Journal.2026-02-21T102143.01.log", -41_183_758),  // -41M CR
            ("Journal.2026-02-25T175718.01.log", -32_124_280),  // -32M CR
            ("Journal.2026-04-06T144830.01.log", -11_300_003),  // -11M CR
            ("Journal.2026-04-07T164157.01.log", -17_300_001),  // -17M CR
            ("Journal.2026-04-10T100037.01.log", -10_150_001)   // -10M CR
        };

        Log(new string('=', 100));
        Log("HUNTING FOR MISSING SPENDING EVENTS");
        Log(new string('=', 100));
        Log("");
        Log("Analyzing journals with large NEGATIVE discrepancies (we're tracking too much income).");
        Log("Looking for events with credit properties that might be spending...");
        Log("");

        foreach (var (targetJournal, expectedDiscrepancy) in journalsToAnalyze)
        {
            var targetFile = journalFiles.FirstOrDefault(f => Path.GetFileName(f) == targetJournal);
            if (targetFile == null)
            {
                Log($"⚠️  Journal not found: {targetJournal}");
                Log("");
                continue;
            }

            var targetIndex = journalFiles.IndexOf(targetFile);
            if (targetIndex == 0)
                continue;

            var prevFile = journalFiles[targetIndex - 1];

            Log(new string('-', 100));
            Log($"📁 {Path.GetFileName(prevFile)} → {Path.GetFileName(targetFile)}");
            Log($"   Expected discrepancy: {expectedDiscrepancy:N0} CR");
            Log("");

            // Parse previous journal
            var prevLines = File.ReadAllLines(prevFile).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
            
            // Find all events with credit properties
            var eventsWithCreditProps = new List<(string eventType, string json, bool isTracked)>();

            foreach (var line in prevLines)
            {
                if (!ContainsCreditProperties(line))
                    continue;

                var doc = JsonDocument.Parse(line);
                var eventType = doc.RootElement.GetProperty("event").GetString() ?? "Unknown";

                // Skip LoadGame, Statistics, and informational events
                if (eventType == "LoadGame" || eventType == "Statistics" || 
                    eventType == "StoredModules" || eventType == "StoredShips" ||
                    eventType == "Loadout" || eventType == "Cargo" || eventType == "Materials" ||
                    eventType == "MissionAccepted" || eventType == "Bounty")
                    continue;

                // Check if this event is tracked
                bool isTracked = false;
                if (parser.TryParseLine(line, out var evt, out _, out _, out _, prevFile))
                {
                    if (evt != null)
                    {
                        var creditChange = CalculateCreditChange(evt);
                        isTracked = creditChange != 0;
                    }
                }

                eventsWithCreditProps.Add((eventType, line, isTracked));
            }

            // Group untracked events
            var untrackedEvents = eventsWithCreditProps
                .Where(e => !e.isTracked)
                .GroupBy(e => e.eventType)
                .OrderByDescending(g => g.Count());

            if (untrackedEvents.Any())
            {
                Log($"   🔍 Found {untrackedEvents.Sum(g => g.Count())} UNTRACKED events with credit properties:");
                Log("");

                foreach (var group in untrackedEvents)
                {
                    Log($"      • {group.Key} ({group.Count()} occurrences)");
                    
                    // Show first example with credit properties extracted
                    var firstExample = group.First();
                    var doc = JsonDocument.Parse(firstExample.json);
                    var creditProps = ExtractCreditProperties(doc.RootElement);
                    
                    if (creditProps.Any())
                    {
                        Log($"        Properties: {string.Join(", ", creditProps)}");
                    }
                }
                Log("");
            }
            else
            {
                Log($"   ✅ No untracked credit events found in this journal");
                Log("");
            }

            // Show tracked spending events for reference
            var trackedSpending = eventsWithCreditProps
                .Where(e => e.isTracked)
                .Select(e => {
                    parser.TryParseLine(e.json, out var evt, out _, out _, out _, prevFile);
                    var change = evt != null ? CalculateCreditChange(evt) : 0;
                    return (e.eventType, change);
                })
                .Where(e => e.change < 0)
                .GroupBy(e => e.eventType)
                .OrderBy(g => g.Sum(x => x.change));

            if (trackedSpending.Any())
            {
                Log($"   💰 Tracked spending in this journal:");
                foreach (var group in trackedSpending)
                {
                    var total = group.Sum(x => x.change);
                    Log($"      • {group.Key} ({group.Count()}x): {total:N0} CR");
                }
                Log("");
            }
        }

        Log(new string('=', 100));
        Log("ANALYSIS COMPLETE");
        Log(new string('=', 100));

        var outputPath = Path.Combine(Path.GetTempPath(), "MissingSpendingEvents.txt");
        File.WriteAllText(outputPath, output.ToString());
        
        Log($"\n\nFull analysis written to: {outputPath}");
        
        Assert.True(true, $"Analysis complete. Details: {outputPath}");
    }

    private bool ContainsCreditProperties(string json)
    {
        var creditKeywords = new[] 
        { 
            "\"Cost\":", "\"Price\":", "\"Reward\":", "\"Amount\":", 
            "\"TotalCost\":", "\"TotalSale\":", "\"TotalEarnings\":",
            "\"BuyPrice\":", "\"SellPrice\":", "\"ShipPrice\":",
            "\"Credits\":", "\"Balance\":", "\"TransferCost\":",
            "\"TransferPrice\":", "\"Deposit\":", "\"Withdraw\":",
            "\"Earnings\":", "\"Payment\":", "\"Fine\":", "\"Bounty\":",
            "\"Value\":", "\"Bonus\":"
        };
        
        return creditKeywords.Any(keyword => json.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

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
                lowerName.Contains("deposit") || lowerName.Contains("withdraw") ||
                lowerName.Contains("payment") || lowerName.Contains("fine") ||
                lowerName.Contains("bounty") || lowerName.Contains("value") ||
                lowerName.Contains("bonus"))
            {
                string value;
                try
                {
                    if (prop.Value.ValueKind == JsonValueKind.Number)
                    {
                        if (prop.Value.TryGetInt64(out var longVal))
                            value = longVal.ToString("N0");
                        else
                            value = prop.Value.GetDouble().ToString("N2");
                    }
                    else
                    {
                        value = prop.Value.ToString();
                    }
                    
                    props.Add($"{name}={value}");
                }
                catch
                {
                    props.Add($"{name}=<parse_error>");
                }
            }
        }
        
        return props;
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
