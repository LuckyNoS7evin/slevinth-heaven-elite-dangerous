namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Model representing an ExoBio discovery
/// </summary>
public class ExoBioDiscoveryModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string ScanType { get; set; } = string.Empty;
    public long SampleValue { get; set; }
    public long EstimatedValue { get; set; }
    public long EstimatedBonus { get; set; }
    public string SystemName { get; set; } = string.Empty;
    public double DistanceFromSol { get; set; }
    public double[]? StarPos { get; set; }
}
