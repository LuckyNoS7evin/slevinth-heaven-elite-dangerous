namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Pure data model representing a single commander rank
/// </summary>
public class RankModel
{
    public string RankType { get; set; } = string.Empty;
    public int RankValue { get; set; }
    public int Progress { get; set; }
}
