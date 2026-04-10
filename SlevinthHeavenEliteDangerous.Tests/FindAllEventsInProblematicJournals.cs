using SlevinthHeavenEliteDangerous.Helpers;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Tests;

/// <summary>
/// Simple test to list ALL event types in journals with large negative discrepancies.
/// This will help us find what spending events we're missing.
/// </summary>
public class FindAllEventsInProblematicJournals
{
    [Fact]
    public void ListAllEventTypesInProblematicJournals()
    {
        var journalPath = GetJournalDirectory();
        
        if (!Directory.Exists(journalPath))
        {
            Assert.True(true, "Journal directory not found - skipping test");
            return;
        }

        var output = new StringBuilder();
        void Log(string message)
        {
            output.AppendLine(message);
            Debug.WriteLine(message);
        }

        // Target journals with large negative discrepancies
        var problematicJournals = new[]
        {
            "Journal.2026-02-20T213145.01.log",  // -41M CR
            "Journal.2026-02-24T180835.01.log",  // -32M CR
            "Journal.2026-03-01T174207.01.log",  // -49M CR
            "Journal.2026-04-06T112405.01.log",  // -11M CR
            "Journal.2026-04-06T144830.01.log",  // -17M CR
        };

        Log("=================================================================");
        Log("SEARCHING FOR ALL EVENT TYPES IN PROBLEMATIC JOURNALS");
        Log("=================================================================");
        Log("");

        foreach (var journalName in problematicJournals)
        {
            var file = Path.Combine(journalPath, journalName);
            if (!File.Exists(file))
            {
                Log($"⚠️  {journalName} NOT FOUND");
                Log("");
                continue;
            }

            Log($"📁 {journalName}");
            Log("");

            var eventCounts = new Dictionary<string, int>();
            var lines = File.ReadAllLines(file);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var doc = JsonDocument.Parse(line);
                    var eventType = doc.RootElement.GetProperty("event").GetString() ?? "Unknown";
                    
                    if (!eventCounts.ContainsKey(eventType))
                        eventCounts[eventType] = 0;
                    
                    eventCounts[eventType]++;
                }
                catch { }
            }

            // Sort by frequency
            var sorted = eventCounts.OrderByDescending(kvp => kvp.Value);

            Log($"  Found {eventCounts.Count} unique event types:");
            Log("");

            foreach (var kvp in sorted)
            {
                Log($"    {kvp.Key,-40} ({kvp.Value,4}x)");
            }

            Log("");
            Log("  🔍 Looking for potential spending events...");
            Log("");

            // Look for events that might be spending-related
            var potentialSpending = eventCounts.Keys.Where(e => 
                e.Contains("Buy") || e.Contains("Pay") || e.Contains("Cost") ||
                e.Contains("Carrier") || e.Contains("Fleet") || e.Contains("Donate") ||
                e.Contains("Tax") || e.Contains("Fee") || e.Contains("Fine") ||
                e.Contains("Transfer") || e.Contains("Engineer") || e.Contains("Tech") ||
                e.Contains("Material") || e.Contains("Apex") || e.Contains("Squadron") ||
                e.Contains("Maintenance") || e.Contains("Repair") || e.Contains("Refuel") ||
                e.Contains("Restock") || e.Contains("Upgrade")
            ).ToList();

            if (potentialSpending.Any())
            {
                foreach (var eventType in potentialSpending)
                {
                    // Check if we track this event
                    var tracked = IsEventTracked(eventType);
                    var icon = tracked ? "✅" : "❌";

                    Log($"    {icon} {eventType} ({eventCounts[eventType]}x) {(tracked ? "[TRACKED]" : "[NOT TRACKED]")}");

                    // Show first example only if NOT tracked
                    if (!tracked)
                    {
                        var example = lines.FirstOrDefault(l => l.Contains($"\"event\":\"{eventType}\""));
                        if (example != null)
                        {
                            try
                            {
                                var doc = JsonDocument.Parse(example);
                                var formatted = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
                                Log($"       Example:");
                                foreach (var fmtLine in formatted.Split('\n').Take(15))
                                {
                                    Log($"       {fmtLine}");
                                }
                            }
                            catch { }
                        }
                        Log("");
                    }
                }
            }
            else
            {
                Log("    ✅ No obvious spending event types found");
            }

            Log("");
            Log(new string('-', 65));
            Log("");
        }

        var outputPath = Path.Combine(Path.GetTempPath(), "AllEventsInProblematicJournals.txt");
        File.WriteAllText(outputPath, output.ToString());
        
        Debug.WriteLine($"\n\nFull output written to: {outputPath}");
        
        Assert.True(true, $"Analysis complete. Output: {outputPath}");
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

    private bool IsEventTracked(string eventType)
    {
        // List of all events we track in CommanderStatsService
        var trackedEvents = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Income
            "MarketSell", "SellExplorationData", "MultiSellExplorationData", "SellOrganicData",
            "RedeemVoucher", "MissionCompleted", "ShipyardSell", "SearchAndRescue",
            "SellDrones", "SellMicroResources", "PowerplaySalary", "CarrierBankTransfer",
            "ModuleSell", "SellSuit", "SellWeapon",
            // Spending
            "MarketBuy", "RefuelAll", "RefuelPartial", "RepairAll", "Repair",
            "PayFines", "PayLegacyFines", "PayBounties", "ShipyardBuy", "ModuleBuy",
            "BuyAmmo", "BuyDrones", "RestockVehicle", "FetchRemoteModule", "ShipyardTransfer",
            "BuyTradeData", "BuyExplorationData", "Resurrect", "BuyMicroResources",
            "BuySuit", "BuyWeapon", "UpgradeSuit", "UpgradeWeapon",
            "NpcCrewPaidWage", "CarrierBuy", "MissionAbandoned", "MissionFailed"
        };

        return trackedEvents.Contains(eventType);
    }
}
