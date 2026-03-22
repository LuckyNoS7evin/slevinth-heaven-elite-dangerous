namespace SlevinthHeavenEliteDangerous.Services.Models;

/// <summary>
/// Represents a signal detected on a body
/// </summary>
public class SignalCard
{
    public string Type_Localised { get; set; } = string.Empty;
    public int Count { get; set; }
}
