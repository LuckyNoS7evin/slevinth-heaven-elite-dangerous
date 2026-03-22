namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Model representing the complete general state
/// </summary>
public class GeneralStateModel
{
    public FSDTimingModel FSDTiming { get; set; } = new();
    public FSDTargetModel FSDTarget { get; set; } = new();
}
