namespace SlevinthHeavenEliteDangerous.Core.Models.Web;

/// <summary>
/// Commander profile as returned by the web API. Fields are filtered
/// based on whether the viewer is the profile owner or another commander.
/// </summary>
public class CommanderProfileDto
{
    public string FID { get; set; } = string.Empty;
    public string CommanderName { get; set; } = string.Empty;

    // Ranks
    public List<RankDto> Ranks { get; set; } = [];

    // Reputation
    public ReputationDto Reputation { get; set; } = new();

    // Stats
    public CommanderStatsDto Stats { get; set; } = new();

    // Visited Systems (filtered by visibility rules)
    public List<VisitedSystemDto> VisitedSystems { get; set; } = [];

    // Codex entries
    public List<CodexEntryDto> CodexEntries { get; set; } = [];

    // ExoBio sales (only sold data visible to others)
    public List<ExoBioSaleDto> ExoBioSales { get; set; } = [];
    public long ExoBioTotalEarnings { get; set; }

    // Current position — only populated for the profile owner
    public string? CurrentSystem { get; set; }
    public string? CurrentStation { get; set; }

    public DateTime LastUpdated { get; set; }
    public bool IsOwnProfile { get; set; }
}

public class RankDto
{
    public string RankType { get; set; } = string.Empty;
    public int RankValue { get; set; }
    public int Progress { get; set; }
    public string RankName { get; set; } = string.Empty;
}

public class ReputationDto
{
    public double Empire { get; set; }
    public double Federation { get; set; }
    public double Independent { get; set; }
    public double Alliance { get; set; }
}

public class CommanderStatsDto
{
    public long CurrentWealth { get; set; }
    // Wallet / bank balance visible only to the profile owner
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

public class VisitedSystemDto
{
    public long SystemAddress { get; set; }
    public string StarSystem { get; set; } = string.Empty;
    public double[]? StarPos { get; set; }
    public bool Discovered { get; set; }
    public List<BodyDto> Bodies { get; set; } = [];
}

public class BodyDto
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

public class CodexEntryDto
{
    public long EntryID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string System { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ExoBioSaleDto
{
    public DateTime SaleTimestamp { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public string StationName { get; set; } = string.Empty;
    public List<BioSaleItemDto> Items { get; set; } = [];
    public long TotalValue { get; set; }
}

public class BioSaleItemDto
{
    public string Species { get; set; } = string.Empty;
    public string Species_Localised { get; set; } = string.Empty;
    public long Value { get; set; }
    public long Bonus { get; set; }
}
