namespace SlevinthHeavenEliteDangerous.Services.Models;

public class CommanderStatsModel
{
    // Bank Account
    public long CurrentWealth { get; set; }
    public int OwnedShipCount { get; set; }

    // Exploration
    public int SystemsVisited { get; set; }
    public long ExplorationProfits { get; set; }
    public int TotalHyperspaceJumps { get; set; }
    public double TotalHyperspaceDistance { get; set; }
    public long TimePlayed { get; set; }

    // Trading
    public long MarketProfits { get; set; }
    public int MarketsTradedWith { get; set; }

    // Combat
    public int BountiesClaimed { get; set; }
    public long BountyHuntingProfit { get; set; }

    // Mining
    public long MiningProfits { get; set; }

    // Exobiology
    public long ExobiologyProfits { get; set; }
    public int OrganicSpeciesAnalysed { get; set; }
}
