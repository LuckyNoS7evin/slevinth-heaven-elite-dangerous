using SlevinthHeavenEliteDangerous.Api.Models;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Api.Processing;

/// <summary>
/// Lightweight server-side processor that reads raw journal JSON lines and
/// updates a <see cref="ServerCommanderData"/> instance incrementally.
/// Uses <see cref="JsonDocument"/> directly — no dependency on the full event type system.
/// </summary>
public static class JournalLineProcessor
{
    /// <summary>
    /// Process a single raw JSON journal line, updating the commander data.
    /// Lines with timestamps older than <see cref="ServerCommanderData.LastJournalTimestamp"/>
    /// are skipped to avoid reprocessing.
    /// </summary>
    public static void ProcessLine(string jsonLine, ServerCommanderData data)
    {
        if (string.IsNullOrWhiteSpace(jsonLine))
            return;

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(jsonLine);
        }
        catch
        {
            return; // silently skip malformed lines
        }

        using (doc)
        {
            var root = doc.RootElement;

            if (!root.TryGetProperty("event", out var eventProp))
                return;

            var eventName = eventProp.GetString();
            if (string.IsNullOrEmpty(eventName))
                return;

            var timestamp = root.TryGetProperty("timestamp", out var ts)
                ? ts.GetDateTime()
                : DateTime.UtcNow;

            switch (eventName)
            {
                case "Commander":
                    ProcessCommander(root, data);
                    break;
                case "LoadGame":
                    ProcessLoadGame(root, data);
                    break;
                case "Rank":
                    ProcessRank(root, data);
                    break;
                case "Progress":
                    ProcessProgress(root, data);
                    break;
                case "Reputation":
                    ProcessReputation(root, data);
                    break;
                case "Statistics":
                    ProcessStatistics(root, data);
                    break;
                case "FSDJump":
                    ProcessFSDJump(root, data, timestamp);
                    break;
                case "Location":
                    ProcessLocation(root, data, timestamp);
                    break;
                case "Scan":
                    ProcessScan(root, data);
                    break;
                case "CodexEntry":
                    ProcessCodexEntry(root, data, timestamp);
                    break;
                case "SellExplorationData":
                    ProcessSellExplorationData(root, data);
                    break;
                case "MultiSellExplorationData":
                    ProcessMultiSellExplorationData(root, data);
                    break;
                case "SellOrganicData":
                    ProcessSellOrganicData(root, data, timestamp);
                    break;
                case "Docked":
                    ProcessDocked(root, data);
                    break;
            }
        }
    }

    // ----- individual processors -----

    private static void ProcessCommander(JsonElement root, ServerCommanderData data)
    {
        data.FID = GetString(root, "FID") ?? data.FID;
        data.CommanderName = GetString(root, "Name") ?? data.CommanderName;
    }

    private static void ProcessLoadGame(JsonElement root, ServerCommanderData data)
    {
        data.FID = GetString(root, "FID") ?? data.FID;
        data.CommanderName = GetString(root, "Commander") ?? data.CommanderName;
    }

    private static void ProcessRank(JsonElement root, ServerCommanderData data)
    {
        foreach (var rankType in RankTypes)
        {
            if (root.TryGetProperty(rankType, out var v) && v.TryGetInt32(out var val))
                data.Ranks[rankType] = val;
        }
    }

    private static void ProcessProgress(JsonElement root, ServerCommanderData data)
    {
        foreach (var rankType in RankTypes)
        {
            if (root.TryGetProperty(rankType, out var v) && v.TryGetInt32(out var val))
                data.RankProgress[rankType] = val;
        }
    }

    private static void ProcessReputation(JsonElement root, ServerCommanderData data)
    {
        data.EmpireReputation = GetDouble(root, "Empire");
        data.FederationReputation = GetDouble(root, "Federation");
        data.IndependentReputation = GetDouble(root, "Independent");
        data.AllianceReputation = GetDouble(root, "Alliance");
    }

    private static void ProcessStatistics(JsonElement root, ServerCommanderData data)
    {
        if (root.TryGetProperty("Bank_Account", out var bank))
        {
            data.Stats.CurrentWealth = GetLong(bank, "Current_Wealth");
            data.Stats.OwnedShipCount = GetInt(bank, "Owned_Ship_Count");
        }

        if (root.TryGetProperty("Exploration", out var explore))
        {
            data.Stats.SystemsVisited = GetInt(explore, "Systems_Visited");
            data.Stats.ExplorationProfits = GetLong(explore, "Exploration_Profits");
            data.Stats.TotalHyperspaceJumps = GetInt(explore, "Total_Hyperspace_Jumps");
            data.Stats.TotalHyperspaceDistance = GetDouble(explore, "Total_Hyperspace_Distance");
            data.Stats.TimePlayed = GetLong(explore, "Time_Played");
        }

        if (root.TryGetProperty("Trading", out var trading))
        {
            data.Stats.MarketProfits = GetLong(trading, "Market_Profits");
            data.Stats.MarketsTradedWith = GetInt(trading, "Markets_Traded_With");
        }

        if (root.TryGetProperty("Combat", out var combat))
        {
            data.Stats.BountiesClaimed = GetInt(combat, "Bounties_Claimed");
            data.Stats.BountyHuntingProfit = GetLong(combat, "Bounty_Hunting_Profit");
        }

        if (root.TryGetProperty("Mining", out var mining))
        {
            data.Stats.MiningProfits = GetLong(mining, "Mining_Profits");
        }

        if (root.TryGetProperty("Exobiology", out var exobio))
        {
            data.Stats.ExobiologyProfits = GetLong(exobio, "Organic_Data_Profits");
            data.Stats.OrganicSpeciesEncountered = GetInt(exobio, "Organic_Species_Encountered");
        }
    }

    private static void ProcessFSDJump(JsonElement root, ServerCommanderData data, DateTime timestamp)
    {
        var system = GetString(root, "StarSystem");
        var address = GetNullableLong(root, "SystemAddress");
        var population = GetLong(root, "Population");

        data.CurrentSystem = system;
        data.CurrentSystemAddress = address;
        data.CurrentStarPos = GetDoubleArray(root, "StarPos");
        data.CurrentStation = null; // no longer docked after a jump

        if (address.HasValue && !string.IsNullOrEmpty(system))
            EnsureVisitedSystem(data, address.Value, system, data.CurrentStarPos, timestamp, population);
    }

    private static void ProcessLocation(JsonElement root, ServerCommanderData data, DateTime timestamp)
    {
        var system = GetString(root, "StarSystem");
        var address = GetNullableLong(root, "SystemAddress");
        var population = GetLong(root, "Population");

        data.CurrentSystem = system;
        data.CurrentSystemAddress = address;
        data.CurrentStarPos = GetDoubleArray(root, "StarPos");

        if (root.TryGetProperty("Docked", out var dockedProp) && dockedProp.GetBoolean())
            data.CurrentStation = GetString(root, "StationName");

        if (address.HasValue && !string.IsNullOrEmpty(system))
            EnsureVisitedSystem(data, address.Value, system, data.CurrentStarPos, timestamp, population);
    }

    private static void ProcessDocked(JsonElement root, ServerCommanderData data)
    {
        data.CurrentStation = GetString(root, "StationName");
    }

    private static void ProcessScan(JsonElement root, ServerCommanderData data)
    {
        var address = GetNullableLong(root, "SystemAddress");
        var bodyId = GetNullableInt(root, "BodyID");
        if (!address.HasValue || !bodyId.HasValue)
            return;

        if (!data.VisitedSystems.TryGetValue(address.Value, out var system))
            return;

        var existing = system.Bodies.FirstOrDefault(b => b.BodyID == bodyId.Value);
        if (existing != null)
            return; // already scanned

        system.Bodies.Add(new ServerBody
        {
            BodyName = GetString(root, "BodyName") ?? string.Empty,
            BodyID = bodyId.Value,
            PlanetClass = GetString(root, "PlanetClass") ?? string.Empty,
            TerraformState = GetString(root, "TerraformState") ?? string.Empty,
            WasDiscovered = GetBool(root, "WasDiscovered"),
            WasMapped = GetBool(root, "WasMapped"),
            Landable = GetBool(root, "Landable"),
            DistanceFromArrivalLS = GetDouble(root, "DistanceFromArrivalLS"),
        });
    }

    private static void ProcessCodexEntry(JsonElement root, ServerCommanderData data, DateTime timestamp)
    {
        if (!GetBool(root, "IsNewEntry"))
            return;

        var entryId = GetNullableLong(root, "EntryID");
        if (!entryId.HasValue)
            return;

        var region = GetString(root, "Region_Localised") ?? GetString(root, "Region") ?? string.Empty;
        var key = $"{entryId}|{region}";

        if (data.CodexEntries.ContainsKey(key))
            return;

        data.CodexEntries[key] = new ServerCodexEntry
        {
            EntryID = entryId.Value,
            Name = GetString(root, "Name_Localised") ?? GetString(root, "Name") ?? string.Empty,
            Category = GetString(root, "Category_Localised") ?? GetString(root, "Category") ?? string.Empty,
            SubCategory = GetString(root, "SubCategory_Localised") ?? GetString(root, "SubCategory") ?? string.Empty,
            Region = region,
            System = GetString(root, "System") ?? string.Empty,
            Timestamp = timestamp,
        };
    }

    private static void ProcessSellExplorationData(JsonElement root, ServerCommanderData data)
    {
        if (root.TryGetProperty("Systems", out var systems) && systems.ValueKind == JsonValueKind.Array)
        {
            foreach (var s in systems.EnumerateArray())
            {
                var name = s.GetString();
                if (!string.IsNullOrEmpty(name))
                    data.SoldNavDataSystems.Add(name);
            }
        }

        MarkNavDataSoldByName(data);
    }

    private static void ProcessMultiSellExplorationData(JsonElement root, ServerCommanderData data)
    {
        if (root.TryGetProperty("Discovered", out var discovered) && discovered.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in discovered.EnumerateArray())
            {
                var name = GetString(entry, "SystemName");
                if (!string.IsNullOrEmpty(name))
                    data.SoldNavDataSystems.Add(name);
            }
        }

        MarkNavDataSoldByName(data);
    }

    private static void ProcessSellOrganicData(JsonElement root, ServerCommanderData data, DateTime timestamp)
    {
        if (!root.TryGetProperty("BioData", out var bioData) || bioData.ValueKind != JsonValueKind.Array)
            return;

        var items = new List<ServerBioSaleItem>();
        long totalValue = 0;

        foreach (var entry in bioData.EnumerateArray())
        {
            var value = GetLong(entry, "Value");
            var bonus = GetLong(entry, "Bonus");
            totalValue += value + bonus;

            items.Add(new ServerBioSaleItem
            {
                Species = GetString(entry, "Species") ?? string.Empty,
                SpeciesLocalised = GetString(entry, "Species_Localised") ?? GetString(entry, "Species") ?? string.Empty,
                Value = value,
                Bonus = bonus,
            });
        }

        data.ExoBioSales.Add(new ServerExoBioSale
        {
            SaleTimestamp = timestamp,
            SystemName = data.CurrentSystem ?? string.Empty,
            StationName = data.CurrentStation ?? string.Empty,
            Items = items,
            TotalValue = totalValue,
        });

        data.ExoBioTotalEarnings += totalValue;
    }

    // ----- helpers -----

    private static void EnsureVisitedSystem(
        ServerCommanderData data, long address, string name, double[]? starPos, DateTime timestamp, long population)
    {
        if (!data.VisitedSystems.TryGetValue(address, out var system))
        {
            system = new ServerVisitedSystem
            {
                SystemAddress = address,
                StarSystem = name,
                StarPos = starPos,
                FirstVisited = timestamp,
                LastVisited = timestamp,
                Population = population,
                NavDataSold = data.SoldNavDataSystems.Contains(name),
            };
            data.VisitedSystems[address] = system;
        }
        else
        {
            system.LastVisited = timestamp;
            if (population > system.Population)
                system.Population = population;
        }
    }

    private static void MarkNavDataSoldByName(ServerCommanderData data)
    {
        foreach (var system in data.VisitedSystems.Values)
        {
            if (!system.NavDataSold && data.SoldNavDataSystems.Contains(system.StarSystem))
                system.NavDataSold = true;
        }
    }

    private static readonly string[] RankTypes =
        ["Combat", "Trade", "Explore", "Soldier", "Exobiologist", "Empire", "Federation", "CQC"];

    private static string? GetString(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static int GetInt(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.TryGetInt32(out var i) ? i : 0;

    private static int? GetNullableInt(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.TryGetInt32(out var i) ? i : null;

    private static long GetLong(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.TryGetInt64(out var l) ? l : 0;

    private static long? GetNullableLong(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.TryGetInt64(out var l) ? l : null;

    private static double GetDouble(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.TryGetDouble(out var d) ? d : 0;

    private static bool GetBool(JsonElement el, string prop) =>
        el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.True;

    private static double[]? GetDoubleArray(JsonElement el, string prop)
    {
        if (!el.TryGetProperty(prop, out var v) || v.ValueKind != JsonValueKind.Array)
            return null;

        var list = new List<double>();
        foreach (var item in v.EnumerateArray())
        {
            if (item.TryGetDouble(out var d))
                list.Add(d);
        }
        return list.Count > 0 ? list.ToArray() : null;
    }
}
