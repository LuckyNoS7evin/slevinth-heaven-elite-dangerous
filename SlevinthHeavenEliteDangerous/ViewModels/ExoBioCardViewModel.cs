using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for an ExoBio discovery card
/// </summary>
public class ExoBioCardViewModel : INotifyPropertyChanged
{
    private string _key = string.Empty;
    private string _title = string.Empty;
    private string _details = string.Empty;
    private string _scanType = string.Empty;
    private long _estimatedValue = 0;
    private long _estimatedBonus = 0;
    private long _sampleValue = 0;

    public string Key
    {
        get => _key;
        set
        {
            if (_key != value)
            {
                _key = value;
                OnPropertyChanged();
            }
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    public string Details
    {
        get => _details;
        set
        {
            if (_details != value)
            {
                _details = value;
                OnPropertyChanged();
            }
        }
    }

    public string ScanType
    {
        get => _scanType;
        set
        {
            if (_scanType != value)
            {
                _scanType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Severity));
                OnPropertyChanged(nameof(CardOpacity));
                OnPropertyChanged(nameof(ShowEstimates));
                OnPropertyChanged(nameof(ShowSampleValue));
            }
        }
    }

    public long EstimatedValue
    {
        get => _estimatedValue;
        set
        {
            if (_estimatedValue != value)
            {
                _estimatedValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstimatedValueFormatted));
            }
        }
    }

    public long EstimatedBonus
    {
        get => _estimatedBonus;
        set
        {
            if (_estimatedBonus != value)
            {
                _estimatedBonus = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EstimatedBonusFormatted));
            }
        }
    }

    public long SampleValue
    {
        get => _sampleValue;
        set
        {
            if (_sampleValue != value)
            {
                _sampleValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SampleValueFormatted));
            }
        }
    }

    public string EstimatedValueFormatted => EstimatedValue.ToString("N0") + " CR";
    public string EstimatedBonusFormatted => EstimatedBonus.ToString("N0") + " CR";
    public string SampleValueFormatted => SampleValue.ToString("N0") + " CR";

    public InfoBarSeverity Severity => ScanType switch
    {
        "Sample" => InfoBarSeverity.Warning,
        "Log" => InfoBarSeverity.Warning,
        "Analyse" => InfoBarSeverity.Success,
        _ => InfoBarSeverity.Informational
    };

    public double CardOpacity => ScanType == "Analyse" ? 1.0 : 0.5;

    public Visibility ShowEstimates => ScanType == "Analyse" ? Visibility.Visible : Visibility.Collapsed;

    public Visibility ShowSampleValue => (ScanType == "Sample" || ScanType == "Log") ? Visibility.Visible : Visibility.Collapsed;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
