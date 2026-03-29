namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Helper for Elite Dangerous combat rank names and kill-to-rank estimation.
/// XP values and rank thresholds are community-documented approximations.
/// </summary>
public static class CombatRankHelper
{
    /// <summary>
    /// Display names for combat ranks 0–8.
    /// </summary>
    public static readonly string[] RankNames =
    [
        "Harmless",
        "Mostly Harmless",
        "Novice",
        "Competent",
        "Expert",
        "Master",
        "Dangerous",
        "Deadly",
        "Elite",
    ];

    /// <summary>
    /// Approximate number of equal-rank kills required to traverse 100 % progress at each rank.
    /// Index matches the current combat rank (0 = Harmless … 7 = Deadly). Index 8 (Elite) has no next rank.
    /// </summary>
    private static readonly int[] KillsPerRankAtEqualRank =
    [
        8,    // Harmless        → Mostly Harmless
        24,   // Mostly Harmless → Novice
        40,   // Novice          → Competent
        80,   // Competent       → Expert
        200,  // Expert          → Master
        400,  // Master          → Dangerous
        800,  // Dangerous       → Deadly
        1600, // Deadly          → Elite
    ];

    /// <summary>
    /// Returns the XP multiplier applied to a kill based on the difference between the
    /// victim's combat rank and the player's combat rank.
    /// </summary>
    public static double GetXpMultiplier(int victimRank, int yourRank)
    {
        int diff = victimRank - yourRank;
        return diff switch
        {
            >= 3 => 4.0,
            2    => 2.0,
            1    => 1.5,
            0    => 1.0,
            -1   => 0.5,
            -2   => 0.25,
            _    => 0.1,  // <= -3
        };
    }

    /// <summary>
    /// Estimates the number of kills needed to reach the next combat rank.
    /// </summary>
    /// <param name="yourRank">Current combat rank integer (0–8).</param>
    /// <param name="progressPercent">Current progress within the rank (0–100).</param>
    /// <param name="victimRank">Combat rank of the opponent being fought.</param>
    /// <returns>Estimated kill count, or null if already at Elite.</returns>
    public static int? EstimateKillsToNextRank(int yourRank, int progressPercent, int victimRank)
    {
        if (yourRank >= 8) return null;

        double remaining = 100.0 - progressPercent;
        double baseKills = KillsPerRankAtEqualRank[yourRank];
        double multiplier = GetXpMultiplier(victimRank, yourRank);

        // One equal-rank kill advances progress by (100 / baseKills) percent.
        // Multiply by the rank-difference multiplier for the actual advancement per kill.
        double progressPerKill = (100.0 / baseKills) * multiplier;

        return (int)Math.Ceiling(remaining / progressPerKill);
    }

    public static string GetRankName(int rank)
    {
        if (rank < 0 || rank >= RankNames.Length) return "Unknown";
        return RankNames[rank];
    }

    public static string GetNextRankName(int rank)
    {
        if (rank >= 8) return "Elite";
        return RankNames[rank + 1];
    }
}
