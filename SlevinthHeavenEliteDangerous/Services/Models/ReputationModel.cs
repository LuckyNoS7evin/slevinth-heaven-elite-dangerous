namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Persisted reputation values for each major faction.
/// Values are in the range -100 to +100.
/// </summary>
public class ReputationModel
{
    public double Empire { get; set; }
    public double Federation { get; set; }
    public double Independent { get; set; }
    public double Alliance { get; set; }
}
