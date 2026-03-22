using SlevinthHeavenEliteDangerous.Events.POCOs;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.Linq;

namespace SlevinthHeavenEliteDangerous.Services;

internal static class BodyValueHelper
{
    public static bool IsEarthLikeWorld(string? planetClass) =>
        !string.IsNullOrWhiteSpace(planetClass)
        && planetClass.Contains("Earth", StringComparison.OrdinalIgnoreCase);

    public static bool IsWaterWorld(string? planetClass) =>
        !string.IsNullOrWhiteSpace(planetClass)
        && planetClass.Contains("Water", StringComparison.OrdinalIgnoreCase)
        && !planetClass.Contains("Gas Giant", StringComparison.OrdinalIgnoreCase);

    public static bool HasTerraformState(string? terraformState) =>
        !string.IsNullOrWhiteSpace(terraformState);

    public static int GetBiologicalSignalCount(IEnumerable<SignalCard> signals) =>
        signals
            .Where(IsBiologicalSignal)
            .Sum(signal => signal.Count);

    public static int GetBiologicalSignalCount(IEnumerable<ViewModels.SignalCardViewModel> signals) =>
        signals
            .Where(IsBiologicalSignal)
            .Sum(signal => signal.Count);

    public static int GetBiologicalSignalCount(IEnumerable<SignalEntry> signals) =>
        signals
            .Where(IsBiologicalSignal)
            .Sum(signal => signal.Count ?? 0);

    private static bool IsBiologicalSignal(SignalCard signal) =>
        signal.Type_Localised.Contains("Biological", StringComparison.OrdinalIgnoreCase);

    private static bool IsBiologicalSignal(ViewModels.SignalCardViewModel signal) =>
        signal.Type_Localised.Contains("Biological", StringComparison.OrdinalIgnoreCase);

    private static bool IsBiologicalSignal(SignalEntry signal) =>
        signal.Type_Localised.Contains("Biological", StringComparison.OrdinalIgnoreCase)
        || signal.Type.Contains("Bio", StringComparison.OrdinalIgnoreCase);
}
