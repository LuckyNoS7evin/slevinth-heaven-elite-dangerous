namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Model representing FSD timing statistics
/// </summary>
public class FSDTimingModel
{
    public int TotalJumps { get; set; }
    public int FastJumpsCount { get; set; }
    public double AvgTimeAllJumps { get; set; }
    public double AvgTimeFastJumps { get; set; }
    public double ShortestTime { get; set; }
    public DateTime? LastJumpTimestamp { get; set; }
}
