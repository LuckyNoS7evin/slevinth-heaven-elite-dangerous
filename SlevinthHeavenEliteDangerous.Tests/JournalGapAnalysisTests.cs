using SlevinthHeavenEliteDangerous.Core.Models;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Helpers;
using System.Diagnostics;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Tests;

/// <summary>
/// Enhanced diagnostic to find missing credit events by comparing adjacent journals.
/// </summary>
public class JournalGapAnalysisTests
{
    [Fact]
    public void AnalyzeJournalTransitionsWithLargeGaps()
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
            Assert.True(true, $"Need at least 2 journal files, found {journalFiles.Count}");
            return;
        }

        var parser = new EventParser();
        var output = new System.Text.StringBuilder();

        void Log(string message)
        {
            output.AppendLine(message);
            Debug.WriteLine(message);
        }

        // Process all journals and identify large gaps
        long calculatedBalance = 0;
        var gaps = new List<(int index, string prevFile, string nextFile, long expected, long actual, long diff)>();

        for (int i = 0; i < journalFiles.Count; i++)
        {
            var file = journalFiles[i];
            var events = new List<EventBase>();

            parser.ParseFile(
                file,
                onEvent: ev => events.Add(ev),
                onUnknown: msg => { },
                onError: (ex, context) => { }
            );

            var currentLoadGame = events.OfType<LoadGameEvent>().FirstOrDefault();

            if (currentLoadGame != null)
            {
                if (i > 0 && calculatedBalance != 0)
                {
                    var expected = calculatedBalance;
                    var actual = currentLoadGame.Credits;
                    var diff = actual - expected;

                    if (Math.Abs(diff) > 100000) // Focus on gaps > 100K CR
                    {
                        gaps.Add((i, Path.GetFileName(journalFiles[i - 1]), Path.GetFileName(file), expected, actual, diff));
                    }
                }

                calculatedBalance = currentLoadGame.Credits;
            }

            // Track all credit changes in this journal
            foreach (var evt in events)
            {
                calculatedBalance += CalculateCreditChange(evt);
            }
        }

        // Analyze each gap
        Log(new string('=', 80));
        Log($"JOURNAL GAP ANALYSIS - Found {gaps.Count} significant gaps");
        Log(new string('=', 80) + "\n");

        foreach (var gap in gaps)
        {
            Log($"\nGAP #{gaps.IndexOf(gap) + 1}: Journal {gap.index}");
            Log($"Previous: {gap.prevFile}");
            Log($"Current:  {gap.nextFile}");
            Log($"Expected: {gap.expected:N0} CR");
            Log($"Actual:   {gap.actual:N0} CR");
            Log($"Diff:     {gap.diff:N0} CR ({(gap.diff > 0 ? "+" : "")}{gap.diff * 100.0 / gap.expected:F2}%)\n");

            // Analyze the PREVIOUS journal's ending
            var prevFile = journalFiles[gap.index - 1];
            var prevEvents = new List<EventBase>();
            var prevLines = new List<string>();

            var lines = File.ReadAllLines(prevFile);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                prevLines.Add(line);
                if (parser.TryParseLine(line, out var evt, out _, out _, out _, prevFile))
                {
                    if (evt != null)
                        prevEvents.Add(evt);
                }
            }

            // Show last 20 events with credit properties
            Log($"  Last 20 events in {gap.prevFile}:");
            var lastCreditEvents = prevLines
                .Where(l => ContainsCreditProperties(l))
                .TakeLast(20)
                .ToList();

            foreach (var line in lastCreditEvents)
            {
                var doc = JsonDocument.Parse(line);
                var eventName = doc.RootElement.GetProperty("event").GetString();
                var timestamp = doc.RootElement.GetProperty("timestamp").GetString();
                var creditProps = ExtractCreditProperties(doc.RootElement);
                
                Log($"    [{timestamp}] {eventName}: {string.Join(", ", creditProps)}");
            }

            // Analyze the CURRENT journal's beginning
            var currFile = journalFiles[gap.index];
            var currLines = File.ReadAllLines(currFile).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            Log($"\n  First 20 events in {gap.nextFile}:");
            var firstCreditEvents = currLines
                .Where(l => ContainsCreditProperties(l))
                .Take(20)
                .ToList();

            foreach (var line in firstCreditEvents)
            {
                var doc = JsonDocument.Parse(line);
                var eventName = doc.RootElement.GetProperty("event").GetString();
                var timestamp = doc.RootElement.GetProperty("timestamp").GetString();
                var creditProps = ExtractCreditProperties(doc.RootElement);
                
                Log($"    [{timestamp}] {eventName}: {string.Join(", ", creditProps)}");
            }

            Log($"\n  {new string('-', 75)}\n");
        }

        // Summary
        Log($"\n{new string('=', 80)}");
        Log("SUMMARY");
        Log(new string('=', 80) + "\n");
        Log($"Total significant gaps found: {gaps.Count}");
        Log($"Total missing credits: {gaps.Sum(g => g.diff):N0} CR");
        Log($"Average gap: {(gaps.Count > 0 ? gaps.Average(g => Math.Abs(g.diff)) : 0):N0} CR\n");

        Log("Possible causes:");
        Log("  1. Fleet Carrier operations (fuel, maintenance, services)");
        Log("  2. Module/Ship transfers not being tracked properly");
        Log("  3. Events occurring during session transitions");
        Log("  4. Game economy changes/corrections");
        Log("  5. Missing event types in CommanderStatsService");

        // Write to file
        var outputPath = Path.Combine(Path.GetTempPath(), "JournalGapAnalysis.txt");
        File.WriteAllText(outputPath, output.ToString());
        Log($"\n\nFull results written to: {outputPath}");
        
        Assert.True(true, $"Analysis complete. Found {gaps.Count} gaps. Results: {outputPath}");
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
                lowerName.Contains("deposit") || lowerName.Contains("withdraw"))
            {
                string value;
                try
                {
                    value = prop.Value.ValueKind switch
                    {
                        JsonValueKind.Number => prop.Value.TryGetInt64(out var longVal)
                            ? longVal.ToString("N0")
                            : prop.Value.GetDouble().ToString("N2"),
                        _ => prop.Value.ToString()
                    };
                }
                catch
                {
                    value = prop.Value.ToString();
                }

                props.Add($"{name}={value}");
            }
        }

        return props;
    }

    private long CalculateCreditChange(EventBase evt)
    {
        return evt switch
        {
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
