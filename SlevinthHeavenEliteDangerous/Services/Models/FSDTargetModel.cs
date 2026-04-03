namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Model representing FSD target information
/// </summary>
public class FSDTargetModel
{
    public string NextSystem { get; set; } = "No Target";
    public int RemainingJumps { get; set; }
    /// <summary>
    /// Final destination of the plotted route. Empty when no multi-hop route is active.
    /// </summary>
    public string FinalDestination { get; set; } = string.Empty;
    /// <summary>
    /// Estimated arrival timestamp in UTC, calculated from remaining jumps and the
    /// average fast-jump time. Null when no estimate is available.
    /// </summary>
    public DateTime? EstimatedArrivalUtc { get; set; }
}
