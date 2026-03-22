using SlevinthHeavenEliteDangerous.Services.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for FSD timing card
/// </summary>
public sealed class FSDTimingViewModel : INotifyPropertyChanged
{
    private int _totalJumps = 0;
    private int _fastJumpsCount = 0;
    private double _avgTimeAllJumps = 0;
    private double _avgTimeFastJumps = 0;
    private double _shortestTime = 0;

    public int TotalJumps
    {
        get => _totalJumps;
        set
        {
            if (_totalJumps != value)
            {
                _totalJumps = value;
                OnPropertyChanged();
            }
        }
    }

    public int FastJumpsCount
    {
        get => _fastJumpsCount;
        set
        {
            if (_fastJumpsCount != value)
            {
                _fastJumpsCount = value;
                OnPropertyChanged();
            }
        }
    }

    public double AvgTimeAllJumps
    {
        get => _avgTimeAllJumps;
        set
        {
            if (_avgTimeAllJumps != value)
            {
                _avgTimeAllJumps = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AvgTimeAllJumpsFormatted));
            }
        }
    }

    public double AvgTimeFastJumps
    {
        get => _avgTimeFastJumps;
        set
        {
            if (_avgTimeFastJumps != value)
            {
                _avgTimeFastJumps = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AvgTimeFastJumpsFormatted));
            }
        }
    }

    public double ShortestTime
    {
        get => _shortestTime;
        set
        {
            if (_shortestTime != value)
            {
                _shortestTime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShortestTimeFormatted));
            }
        }
    }

    public string AvgTimeAllJumpsFormatted => FormatTimeSpan(AvgTimeAllJumps);
    public string AvgTimeFastJumpsFormatted => FormatTimeSpan(AvgTimeFastJumps);
    public string ShortestTimeFormatted => FormatTimeSpan(ShortestTime);

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void UpdateFromModel(FSDTimingModel model)
    {
        TotalJumps = model.TotalJumps;
        FastJumpsCount = model.FastJumpsCount;
        AvgTimeAllJumps = model.AvgTimeAllJumps;
        AvgTimeFastJumps = model.AvgTimeFastJumps;
        ShortestTime = model.ShortestTime;
    }

    private static string FormatTimeSpan(double seconds)
    {
        if (seconds == 0)
        {
            return "N/A";
        }

        var timeSpan = TimeSpan.FromMilliseconds(seconds);

        if (timeSpan.TotalMinutes < 1)
        {
            return $"{timeSpan.Seconds}s {timeSpan.Milliseconds}ms";
        }
        else if (timeSpan.TotalHours < 1)
        {
            return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
        else
        {
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        }
    }
}
