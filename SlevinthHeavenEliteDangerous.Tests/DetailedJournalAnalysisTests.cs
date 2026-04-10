using SlevinthHeavenEliteDangerous.Core.Models;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Helpers;
using System.Diagnostics;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Tests;

/// <summary>
/// Deep dive analysis of a specific journal pair to find ALL untracked credit events.
/// </summary>
public class DetailedJournalAnalysisTests
{
    [Fact]
    public void DeepAnalysisOfWorstGap()
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

        // Analyze the worst gap: Journal 35 (49M CR difference)
        var targetJournal = "Journal.2026-03-04T172930.01.log";
        var journalIndex = journalFiles.FindIndex(f => Path.GetFileName(f) == targetJournal);
        
        if (journalIndex <= 0)
        {
            Assert.True(true, $"Target journal {targetJournal} not found or is first journal");
            return;
        }

        var prevFile = journalFiles[journalIndex - 1];
        var currFile = journalFiles[journalIndex];
        
        var output = new System.Text.StringBuilder();
        void Log(string message)
        {
            output.AppendLine(message);
            Debug.WriteLine(message);
        }

        var parser = new EventParser();

        // Subscribe to serialization failures to debug SellOrganicData issues
        parser.SerializationFailed += (failure) =>
        {
            Log($"SERIALIZATION ERROR: {failure.EventName} - {failure.Error}");
            if (failure.EventName == "SellOrganicData")
            {
                var maxLen = Math.Min(500, failure.RawJson?.Length ?? 0);
                Log($"  RAW JSON (first {maxLen} chars): {failure.RawJson?.Substring(0, maxLen)}");
            }
        };

        Log(new string('=', 90));
        Log($"DETAILED ANALYSIS: {Path.GetFileName(prevFile)} → {Path.GetFileName(targetJournal)}");
        Log(new string('=', 90));
        Log("");

        // Parse previous journal
        var prevEvents = new List<EventBase>();
        var prevLines = File.ReadAllLines(prevFile).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        
        foreach (var line in prevLines)
        {
            if (parser.TryParseLine(line, out var evt, out _, out _, out _, prevFile))
            {
                if (evt != null)
                    prevEvents.Add(evt);
            }
        }

        var prevLoadGame = prevEvents.OfType<LoadGameEvent>().FirstOrDefault();
        var startBalance = prevLoadGame?.Credits ?? 0;

        Log($"PREVIOUS JOURNAL: {Path.GetFileName(prevFile)}");
        Log($"Starting balance (LoadGame): {startBalance:N0} CR\n");

        // Track all events and categorize them
        var trackedEvents = new List<(EventBase evt, long change, string category)>();
        var untrackedCreditEvents = new List<(string eventType, string json, List<string> creditProps)>();

        foreach (var line in prevLines)
        {
            // Check for SellOrganicData first to add special debugging
            if (line.Contains("\"SellOrganicData\""))
            {
                Log($"DEBUG: Found SellOrganicData JSON line");
            }

            if (!ContainsCreditProperties(line))
                continue;

            var parsed = false;
            EventBase? evt = null;

            if (parser.TryParseLine(line, out evt, out var eventName, out _, out _, prevFile))
            {
                parsed = true;
                if (eventName == "SellOrganicData")
                {
                    Log($"DEBUG: Successfully parsed SellOrganicData event");
                }
            }

            var creditChange = evt != null ? CalculateCreditChange(evt) : 0;

            // Special debugging for SellOrganicData
            if (evt is SellOrganicDataEvent sellOrganic)
            {
                Log($"DEBUG: SellOrganicData cast successful. BioData is {(sellOrganic.BioData == null ? "null" : $"not null with {sellOrganic.BioData.Count} entries")}");
                if (sellOrganic.BioData.Count > 0)
                {
                    var total = sellOrganic.BioData.Sum(b => b.Value + b.Bonus);
                    Log($"DEBUG: Total credits from SellOrganicData: {total:N0} CR");
                }
            }

            if (creditChange != 0)
            {
                var category = evt?.GetType().Name ?? "Unknown";
                trackedEvents.Add((evt!, creditChange, category));
            }
            else if (parsed && ContainsCreditProperties(line))
            {
                // This event has credit properties but we're not tracking it!
                var doc = JsonDocument.Parse(line);
                var evtType = doc.RootElement.GetProperty("event").GetString() ?? "Unknown";
                var creditProps = ExtractCreditProperties(doc.RootElement);
                
                if (creditProps.Count > 0 && evtType != "LoadGame" && evtType != "Statistics" && 
                    evtType != "StoredModules" && evtType != "StoredShips" && evtType != "Bounty" && evtType != "MissionAccepted")
                {
                    untrackedCreditEvents.Add((evtType, line, creditProps));
                }
            }
        }

        var calculatedEnd = startBalance + trackedEvents.Sum(e => e.change);

        // Parse current journal
        var currEvents = new List<EventBase>();
        parser.ParseFile(currFile, evt => currEvents.Add(evt), null, null);
        var currLoadGame = currEvents.OfType<LoadGameEvent>().FirstOrDefault();
        var actualNextBalance = currLoadGame?.Credits ?? 0;

        var discrepancy = actualNextBalance - calculatedEnd;

        Log($"Tracked {trackedEvents.Count} credit-changing events:");
        
        var grouped = trackedEvents.GroupBy(e => e.category).OrderByDescending(g => Math.Abs(g.Sum(x => x.change)));
        foreach (var group in grouped)
        {
            var sum = group.Sum(x => x.change);
            var sign = sum > 0 ? "+" : "";
            Log($"  {group.Key,-30} ({group.Count(),3}x): {sign}{sum,15:N0} CR");
        }

        var totalChange = trackedEvents.Sum(e => e.change);
        Log($"\n  {"TOTAL TRACKED CHANGE",-30}        {(totalChange > 0 ? "+" : "")}{totalChange,15:N0} CR");
        Log($"  {"Calculated end balance",-30}        {calculatedEnd,16:N0} CR");
        Log("");

        Log($"NEXT JOURNAL: {Path.GetFileName(targetJournal)}");
        Log($"Actual balance (LoadGame):   {actualNextBalance:N0} CR\n");

        Log($"DISCREPANCY: {discrepancy:N0} CR ({(discrepancy > 0 ? "+" : "")}{discrepancy * 100.0 / calculatedEnd:F2}%)\n");

        if (untrackedCreditEvents.Count > 0)
        {
            Log(new string('-', 90));
            Log($"FOUND {untrackedCreditEvents.Count} UNTRACKED EVENTS WITH CREDIT PROPERTIES:\n");

            var groupedUntracked = untrackedCreditEvents.GroupBy(e => e.eventType);
            foreach (var group in groupedUntracked)
            {
                Log($"Event: {group.Key} ({group.Count()} occurrences)");
                
                var first = group.First();
                Log($"  Properties: {string.Join(", ", first.creditProps)}");
                Log($"  Example JSON:");
                
                var doc = JsonDocument.Parse(first.json);
                var formatted = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
                foreach (var line in formatted.Split('\n').Take(20))
                {
                    Log($"    {line}");
                }
                
                Log("");
            }
        }
        else
        {
            Log(new string('-', 90));
            Log("NO OBVIOUSLY UNTRACKED EVENTS FOUND");
            Log("\nPossible reasons for discrepancy:");
            Log("  1. Events with credit properties that don't match our patterns");
            Log("  2. Incorrect calculation in tracked events (wrong property used)");
            Log("  3. Game economy adjustments between sessions");
            Log("  4. Events that modify balance without obvious credit properties");
            Log("");
            Log("Recommendation: Review ALL events in the journal, especially near the end:");
        }

        // Show last 30 events regardless
        Log(new string('-', 90));
        Log($"LAST 30 EVENTS IN {Path.GetFileName(prevFile)}:\n");
        
        var lastEvents = prevLines.TakeLast(30);
        foreach (var line in lastEvents)
        {
            var doc = JsonDocument.Parse(line);
            var evtType = doc.RootElement.GetProperty("event").GetString();
            var timestamp = doc.RootElement.GetProperty("timestamp").GetString();
            var creditProps = ExtractCreditProperties(doc.RootElement);
            
            var propStr = creditProps.Count > 0 ? $" → {string.Join(", ", creditProps)}" : "";
            Log($"  [{timestamp}] {evtType}{propStr}");
        }

        // Write to file
        var outputPath = Path.Combine(Path.GetTempPath(), "DetailedJournalAnalysis.txt");
        File.WriteAllText(outputPath, output.ToString());
        
        Log($"\n\nFull analysis written to: {outputPath}");
        
        Assert.True(true, $"Analysis complete. Discrepancy: {discrepancy:N0} CR. Details: {outputPath}");
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
                lowerName.Contains("bounty"))
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
            MissionAbandonedEvent missionAbandoned when missionAbandoned.Fine.HasValue => -missionAbandoned.Fine.Value,
            MissionFailedEvent missionFailed when missionFailed.Fine.HasValue => -missionFailed.Fine.Value,
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
