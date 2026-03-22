using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for a single faction's reputation card.
/// </summary>
public class FactionReputationViewModel : INotifyPropertyChanged
{
    private double _value;

    public string FactionName { get; init; } = string.Empty;

    public double Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ValueFormatted));
                OnPropertyChanged(nameof(StatusLabel));
                OnPropertyChanged(nameof(BarValue));
            }
        }
    }

    /// <summary>Value as a signed percentage string, e.g. "+64.2%" or "-12.0%".</summary>
    public string ValueFormatted => $"{_value:+0.0;-0.0;0.0}%";

    /// <summary>Human-readable standing label.</summary>
    public string StatusLabel => _value switch
    {
        >= 63  => "Allied",
        >= 24  => "Friendly",
        >= 4   => "Cordial",
        >= -4  => "Neutral",
        >= -35 => "Unfriendly",
        _      => "Hostile"
    };

    /// <summary>Progress bar value mapped from [-100, +100] → [0, 100].</summary>
    public double BarValue => (_value + 100.0) / 2.0;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
