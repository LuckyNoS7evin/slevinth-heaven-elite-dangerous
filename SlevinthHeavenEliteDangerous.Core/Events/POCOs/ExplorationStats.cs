using System.Text.Json.Serialization;

namespace SlevinthHeavenEliteDangerous.Events.POCOs;

public class ExplorationStats
{
    [JsonPropertyName("Systems_Visited")]
    public int? SystemsVisited { get; set; }

    [JsonPropertyName("Exploration_Profits")]
    public long? ExplorationProfits { get; set; }

    [JsonPropertyName("Planets_Scanned_To_Level_2")]
    public int? PlanetsScannedToLevel2 { get; set; }

    [JsonPropertyName("Planets_Scanned_To_Level_3")]
    public int? PlanetsScannedToLevel3 { get; set; }

    [JsonPropertyName("Efficient_Scans")]
    public int? EfficientScans { get; set; }

    [JsonPropertyName("Highest_Payout")]
    public long? HighestPayout { get; set; }

    [JsonPropertyName("Total_Hyperspace_Distance")]
    public double? TotalHyperspaceDistance { get; set; }

    [JsonPropertyName("Total_Hyperspace_Jumps")]
    public int? TotalHyperspaceJumps { get; set; }

    [JsonPropertyName("Greatest_Distance_From_Start")]
    public double? GreatestDistanceFromStart { get; set; }

    [JsonPropertyName("Time_Played")]
    public long? TimePlayed { get; set; }
}
