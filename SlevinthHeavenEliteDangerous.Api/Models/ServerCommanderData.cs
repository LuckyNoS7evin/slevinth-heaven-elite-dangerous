using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Api.Models;

/// <summary>
/// Complete server-side commander data built from uploaded journal lines.
/// Persisted as a JSON file per commander (keyed by Frontier ID).
/// </summary>
public class ServerCommanderData
{
    // Identity
    public string FID { get; set; } = string.Empty;
    public string CommanderName { get; set; } = string.Empty;

    // Current position — private, never shared with other commanders
    public string? CurrentSystem { get; set; }
    public long? CurrentSystemAddress { get; set; }
    public double[]? CurrentStarPos { get; set; }
    public string? CurrentStation { get; set; }

    // Ranks  (key = rank type name, e.g. "Combat")
    public Dictionary<string, int> Ranks { get; set; } = new();
    public Dictionary<string, int> RankProgress { get; set; } = new();

    // Reputation
    public double EmpireReputation { get; set; }
    public double FederationReputation { get; set; }
    public double IndependentReputation { get; set; }
    public double AllianceReputation { get; set; }

    // Stats
    public ServerCommanderStats Stats { get; set; } = new();

    // Visited systems (key = SystemAddress)
    public Dictionary<long, ServerVisitedSystem> VisitedSystems { get; set; } = new();

    // Systems where nav data has been sold (StarSystem names)
    // Use case-insensitive comparer to match journal names regardless of localisation/casing.
    public HashSet<string> SoldNavDataSystems { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Codex entries (key = "{EntryID}|{Region}")
    public Dictionary<string, ServerCodexEntry> CodexEntries { get; set; } = new();

    // ExoBio
    public List<ServerExoBioSale> ExoBioSales { get; set; } = [];
    public long ExoBioTotalEarnings { get; set; }

    // Metadata
    public DateTime LastUploadTimestamp { get; set; }
    public string? LastAppVersion { get; set; }
}

public class ServerCommanderStats
{
    // Net worth as reported by the Statistics event
    public long CurrentWealth { get; set; }
    // Wallet / bank balance reported from LoadGameEvent Credits (private)
    public long WalletBalance { get; set; }
    public int OwnedShipCount { get; set; }
    public int SystemsVisited { get; set; }
    public long ExplorationProfits { get; set; }
    public int TotalHyperspaceJumps { get; set; }
    public double TotalHyperspaceDistance { get; set; }
    public long TimePlayed { get; set; }
    public long MarketProfits { get; set; }
    public int MarketsTradedWith { get; set; }
    public int BountiesClaimed { get; set; }
    public long BountyHuntingProfit { get; set; }
    public long MiningProfits { get; set; }
    public long ExobiologyProfits { get; set; }
    public int OrganicSpeciesEncountered { get; set; }
}

public class ServerVisitedSystem
{
    public long SystemAddress { get; set; }
    public string StarSystem { get; set; } = string.Empty;
    public double[]? StarPos { get; set; }
    public DateTime FirstVisited { get; set; }
    public DateTime LastVisited { get; set; }
    public bool NavDataSold { get; set; }
    public long Population { get; set; }
    public List<ServerBody> Bodies { get; set; } = [];
}

public class ServerBody
{
    public string BodyName { get; set; } = string.Empty;
    public int BodyID { get; set; }
    public string PlanetClass { get; set; } = string.Empty;
    public string TerraformState { get; set; } = string.Empty;
    public bool WasDiscovered { get; set; }
    public bool WasMapped { get; set; }
    public bool Landable { get; set; }
    public double DistanceFromArrivalLS { get; set; }
}

public class ServerCodexEntry
{
    public long EntryID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string System { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ServerExoBioSale
{
    public DateTime SaleTimestamp { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public List<ServerBioSaleItem> Items { get; set; } = [];
    public long TotalValue { get; set; }
}

public class ServerBioSaleItem
{
    public string Species { get; set; } = string.Empty;

    [JsonPropertyName("Species_Localised")]
    public string SpeciesLocalised { get; set; } = string.Empty;

    public long Value { get; set; }
    public long Bonus { get; set; }
}
