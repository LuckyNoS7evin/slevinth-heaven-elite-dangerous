using SlevinthHeavenEliteDangerous.Services.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for FSD target card
/// </summary>
public sealed class FSDTargetViewModel : INotifyPropertyChanged
{
    private string _nextSystem = "No Target";
    private int _remainingJumps = 0;
    private string _finalDestination = string.Empty;

    public string NextSystem
    {
        get => _nextSystem;
        set
        {
            if (_nextSystem != value)
            {
                _nextSystem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasFinalDestination));
            }
        }
    }

    public int RemainingJumps
    {
        get => _remainingJumps;
        set
        {
            if (_remainingJumps != value)
            {
                _remainingJumps = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainingJumpsFormatted));
                OnPropertyChanged(nameof(HasFinalDestination));
            }
        }
    }

    public string FinalDestination
    {
        get => _finalDestination;
        set
        {
            if (_finalDestination != value)
            {
                _finalDestination = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasFinalDestination));
            }
        }
    }

    public string RemainingJumpsFormatted => RemainingJumps > 0 ? RemainingJumps.ToString() : "-";

    /// <summary>
    /// True when a multi-hop route is active and the final destination differs from the immediate next system.
    /// </summary>
    public bool HasFinalDestination =>
        !string.IsNullOrEmpty(FinalDestination)
        && RemainingJumps > 1
        && !string.Equals(FinalDestination, NextSystem, StringComparison.OrdinalIgnoreCase);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateFromModel(FSDTargetModel model)
    {
        NextSystem = model.NextSystem;
        RemainingJumps = model.RemainingJumps;
        FinalDestination = model.FinalDestination;
    }

    public void ClearTarget()
    {
        NextSystem = "No Target";
        RemainingJumps = 0;
        FinalDestination = string.Empty;
    }
}
