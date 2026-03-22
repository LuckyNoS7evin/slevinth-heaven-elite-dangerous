namespace SlevinthHeavenEliteDangerous.Data;

/// <summary>
/// Elite Dangerous rank names for all rank types
/// </summary>
public static class RankNames
{
    public static readonly string[] Combat = 
    [
        "Harmless",
        "Mostly Harmless",
        "Novice",
        "Competent",
        "Expert",
        "Master",
        "Dangerous",
        "Deadly",
        "Elite"
    ];

    public static readonly string[] Trade = 
    [
        "Penniless",
        "Mostly Penniless",
        "Peddler",
        "Dealer",
        "Merchant",
        "Broker",
        "Entrepreneur",
        "Tycoon",
        "Elite"
    ];

    public static readonly string[] Explore = 
    [
        "Aimless",
        "Mostly Aimless",
        "Scout",
        "Surveyor",
        "Trailblazer",
        "Pathfinder",
        "Ranger",
        "Pioneer",
        "Elite"
    ];

    public static readonly string[] Soldier = 
    [
        "Defenceless",
        "Mostly Defenceless",
        "Rookie",
        "Soldier",
        "Gunslinger",
        "Warrior",
        "Gladiator",
        "Deadeye",
        "Elite"
    ];

    public static readonly string[] Exobiologist = 
    [
        "Directionless",
        "Mostly Directionless",
        "Compiler",
        "Collector",
        "Cataloguer",
        "Taxonomist",
        "Ecologist",
        "Geneticist",
        "Elite"
    ];

    public static readonly string[] Empire = 
    [
        "None",
        "Outsider",
        "Serf",
        "Master",
        "Squire",
        "Knight",
        "Lord",
        "Baron",
        "Viscount",
        "Count",
        "Earl",
        "Marquis",
        "Duke",
        "Prince",
        "King"
    ];

    public static readonly string[] Federation = 
    [
        "None",
        "Recruit",
        "Cadet",
        "Midshipman",
        "Petty Officer",
        "Chief Petty Officer",
        "Warrant Officer",
        "Ensign",
        "Lieutenant",
        "Lieutenant Commander",
        "Post Commander",
        "Post Captain",
        "Rear Admiral",
        "Vice Admiral",
        "Admiral"
    ];

    public static readonly string[] CQC = 
    [
        "Helpless",
        "Mostly Helpless",
        "Amateur",
        "Semi Professional",
        "Professional",
        "Champion",
        "Hero",
        "Legend",
        "Elite"
    ];

    public static string GetRankName(string rankType, int rankValue)
    {
        var ranks = rankType switch
        {
            "Combat" => Combat,
            "Trade" => Trade,
            "Explore" => Explore,
            "Soldier" => Soldier,
            "Exobiologist" => Exobiologist,
            "Empire" => Empire,
            "Federation" => Federation,
            "CQC" => CQC,
            _ => null
        };

        if (ranks == null || rankValue < 0 || rankValue >= ranks.Length)
        {
            return "Unknown";
        }

        return ranks[rankValue];
    }
}
