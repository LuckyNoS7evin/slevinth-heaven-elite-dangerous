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
    private DateTime? _estimatedArrivalUtc;

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

    public DateTime? EstimatedArrivalUtc
    {
        get => _estimatedArrivalUtc;
        set
        {
            if (_estimatedArrivalUtc != value)
            {
                _estimatedArrivalUtc = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstimatedArrivalFormatted));
                OnPropertyChanged(nameof(RemainingJumpsFormatted));
            }
        }
    }

    public string RemainingJumpsFormatted
    {
        get
        {
            if (RemainingJumps <= 0) return "-";

            if (EstimatedArrivalUtc.HasValue)
            {
                return $"{RemainingJumps} (ETA {EstimatedArrivalFormatted})";
            }

            return RemainingJumps.ToString();
        }
    }

    /// <summary>
    /// Formats the estimated arrival time for display. Uses `HH:mm:ss` when the
    /// remaining duration contains an hour segment; otherwise uses `mm:ss` as requested.
    /// </summary>
    public string EstimatedArrivalFormatted
    {
        get
        {
            if (!EstimatedArrivalUtc.HasValue) return "-";

            var arrivalLocal = EstimatedArrivalUtc.Value.ToLocalTime();
            var remaining = EstimatedArrivalUtc.Value - DateTime.UtcNow;

            if (remaining.TotalSeconds <= 0)
                return arrivalLocal.ToString("HH:mm:ss");

            if (remaining.TotalHours >= 1)
                return arrivalLocal.ToString("HH:mm:ss");

            // No hour segment -> show minute:second of the arrival time
            return arrivalLocal.ToString("mm:ss");
        }
    }

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
        EstimatedArrivalUtc = model.EstimatedArrivalUtc;
    }

    public void ClearTarget()
    {
        NextSystem = "No Target";
        RemainingJumps = 0;
        FinalDestination = string.Empty;
        EstimatedArrivalUtc = null;
    }
}
