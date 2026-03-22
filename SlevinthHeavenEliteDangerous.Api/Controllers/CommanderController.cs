using Microsoft.AspNetCore.Mvc;
using SlevinthHeavenEliteDangerous.Api.Models;
using SlevinthHeavenEliteDangerous.Api.Storage;
using SlevinthHeavenEliteDangerous.Core.Models.Web;

namespace SlevinthHeavenEliteDangerous.Api.Controllers;

/// <summary>
/// Public API for querying commander profiles.
/// Visibility rules are applied based on whether the caller owns the profile.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CommanderController(CommanderDataStore store) : ControllerBase
{
    private static readonly string[] RankTypes =
        ["Combat", "Trade", "Explore", "Soldier", "Exobiologist", "Empire", "Federation", "CQC"];

    private static readonly Dictionary<string, string[]> RankNameLookup = new()
    {
        ["Combat"] = ["Harmless", "Mostly Harmless", "Novice", "Competent", "Expert", "Master", "Dangerous", "Deadly", "Elite"],
        ["Trade"] = ["Penniless", "Mostly Penniless", "Peddler", "Dealer", "Merchant", "Broker", "Entrepreneur", "Tycoon", "Elite"],
        ["Explore"] = ["Aimless", "Mostly Aimless", "Scout", "Surveyor", "Trailblazer", "Pathfinder", "Ranger", "Pioneer", "Elite"],
        ["Soldier"] = ["Defenceless", "Mostly Defenceless", "Rookie", "Soldier", "Gunslinger", "Warrior", "Gladiator", "Deadeye", "Elite"],
        ["Exobiologist"] = ["Directionless", "Mostly Directionless", "Compiler", "Collector", "Cataloguer", "Taxonomist", "Ecologist", "Geneticist", "Elite"],
        ["Empire"] = ["None", "Outsider", "Serf", "Master", "Squire", "Knight", "Lord", "Baron", "Viscount", "Count", "Earl", "Marquis", "Duke", "Prince", "King"],
        ["Federation"] = ["None", "Recruit", "Cadet", "Midshipman", "Petty Officer", "Chief Petty Officer", "Warrant Officer", "Ensign", "Lieutenant", "Lieutenant Commander", "Post Commander", "Post Captain", "Rear Admiral", "Vice Admiral", "Admiral"],
        ["CQC"] = ["Helpless", "Mostly Helpless", "Amateur", "Semi Professional", "Professional", "Champion", "Hero", "Legend", "Elite"],
    };

    /// <summary>
    /// List all commanders with basic public info.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListCommanders()
    {
        var all = await store.GetAllAsync();

        var summaries = all.Select(d => new
        {
            d.FID,
            d.CommanderName,
            d.LastUploadTimestamp,
        }).ToList();

        return Ok(summaries);
    }

    /// <summary>
    /// Get the full profile for the currently authenticated commander.
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var commanderName = HttpContext.Items["CommanderName"] as string;
        if (string.IsNullOrEmpty(commanderName))
            return Unauthorized();

        var all = await store.GetAllAsync();
        var data = all.FirstOrDefault(d =>
            string.Equals(d.CommanderName, commanderName, StringComparison.OrdinalIgnoreCase));

        if (data is null)
            return NotFound("No journal data uploaded yet.");

        return Ok(ToDto(data, isOwner: true));
    }

    /// <summary>
    /// Get a commander profile by FID. Visibility rules are applied.
    /// </summary>
    [HttpGet("{fid}")]
    public async Task<IActionResult> GetProfile(string fid)
    {
        var data = await store.GetAsync(fid);
        if (data is null)
            return NotFound();

        var callerName = HttpContext.Items["CommanderName"] as string;
        // Also check cookie auth
        if (string.IsNullOrEmpty(callerName))
            callerName = HttpContext.User.Identity?.Name;

        var isOwner = !string.IsNullOrEmpty(callerName) &&
                      string.Equals(data.CommanderName, callerName, StringComparison.OrdinalIgnoreCase);

        return Ok(ToDto(data, isOwner));
    }

    // ---- mapping with visibility rules ----

    private static CommanderProfileDto ToDto(ServerCommanderData data, bool isOwner)
    {
        var dto = new CommanderProfileDto
        {
            FID = data.FID,
            CommanderName = data.CommanderName,
            IsOwnProfile = isOwner,
            LastUpdated = data.LastUploadTimestamp,

            // Ranks — always visible
            Ranks = RankTypes.Select(rt => new RankDto
            {
                RankType = rt,
                RankValue = data.Ranks.TryGetValue(rt, out var v) ? v : 0,
                Progress = data.RankProgress.TryGetValue(rt, out var p) ? p : 0,
                RankName = GetRankName(rt, data.Ranks.TryGetValue(rt, out var rv) ? rv : 0),
            }).ToList(),

            // Reputation — always visible
            Reputation = new ReputationDto
            {
                Empire = data.EmpireReputation,
                Federation = data.FederationReputation,
                Independent = data.IndependentReputation,
                Alliance = data.AllianceReputation,
            },

            // Stats — bounty info hidden for non-owners
            Stats = new CommanderStatsDto
            {
                CurrentWealth = data.Stats.CurrentWealth,
                OwnedShipCount = data.Stats.OwnedShipCount,
                SystemsVisited = data.Stats.SystemsVisited,
                ExplorationProfits = data.Stats.ExplorationProfits,
                TotalHyperspaceJumps = data.Stats.TotalHyperspaceJumps,
                TotalHyperspaceDistance = data.Stats.TotalHyperspaceDistance,
                TimePlayed = data.Stats.TimePlayed,
                MarketProfits = data.Stats.MarketProfits,
                MarketsTradedWith = data.Stats.MarketsTradedWith,
                BountiesClaimed = isOwner ? data.Stats.BountiesClaimed : 0,
                BountyHuntingProfit = isOwner ? data.Stats.BountyHuntingProfit : 0,
                MiningProfits = data.Stats.MiningProfits,
                ExobiologyProfits = data.Stats.ExobiologyProfits,
                OrganicSpeciesEncountered = data.Stats.OrganicSpeciesEncountered,
            },

            // Codex — system and timestamp hidden for non-owners
            CodexEntries = data.CodexEntries.Values.Select(c => new CodexEntryDto
            {
                EntryID = c.EntryID,
                Name = c.Name,
                Category = c.Category,
                SubCategory = c.SubCategory,
                Region = c.Region,
                System = isOwner ? c.System : string.Empty,
                Timestamp = isOwner ? c.Timestamp : default,
            }).OrderByDescending(c => c.Timestamp).ToList(),

            // ExoBio sales — system and station hidden for non-owners
            ExoBioSales = data.ExoBioSales.Select(s => new ExoBioSaleDto
            {
                SaleTimestamp = s.SaleTimestamp,
                SystemName = isOwner ? s.SystemName : string.Empty,
                StationName = isOwner ? s.StationName : string.Empty,
                TotalValue = s.TotalValue,
                Items = s.Items.Select(i => new BioSaleItemDto
                {
                    Species = i.Species,
                    Species_Localised = i.SpeciesLocalised,
                    Value = i.Value,
                    Bonus = i.Bonus,
                }).ToList(),
            }).OrderByDescending(s => s.SaleTimestamp).ToList(),

            ExoBioTotalEarnings = data.ExoBioTotalEarnings,
        };

        // Visited systems — visibility depends on ownership
        if (isOwner)
        {
            // Owner sees all systems — undiscovered first, then alphabetically
            dto.VisitedSystems = data.VisitedSystems.Values.Select(MapVisitedSystem)
                .OrderBy(s => s.Discovered)
                .ThenBy(s => s.StarSystem)
                .ToList();

            // Owner sees current position
            dto.CurrentSystem = data.CurrentSystem;
            dto.CurrentStation = data.CurrentStation;
        }
        else
        {
            // Others see systems where nav data has been sold or first discoveries were made
            dto.VisitedSystems = data.VisitedSystems.Values
                .Where(s => s.NavDataSold || s.Population > 0 || s.Bodies.Any(b => b.WasDiscovered))
                .Select(MapVisitedSystem)
                .OrderBy(s => s.StarSystem)
                .ToList();

            // Current position is hidden
            dto.CurrentSystem = null;
            dto.CurrentStation = null;
        }

        return dto;
    }

    private static VisitedSystemDto MapVisitedSystem(ServerVisitedSystem s) => new()
    {
        SystemAddress = s.SystemAddress,
        StarSystem = s.StarSystem,
        Discovered = s.NavDataSold || s.Population > 0 || s.Bodies.Any(b => b.WasDiscovered),
        StarPos = s.StarPos,
        Bodies = s.Bodies.Select(b => new BodyDto
        {
            BodyName = b.BodyName,
            BodyID = b.BodyID,
            PlanetClass = b.PlanetClass,
            TerraformState = b.TerraformState,
            WasDiscovered = b.WasDiscovered,
            WasMapped = b.WasMapped,
            Landable = b.Landable,
            DistanceFromArrivalLS = b.DistanceFromArrivalLS,
        }).ToList(),
    };

    private static string GetRankName(string rankType, int value)
    {
        if (RankNameLookup.TryGetValue(rankType, out var names) && value >= 0 && value < names.Length)
            return names[value];
        return "Unknown";
    }
}
