using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for a single commander rank — owns INotifyPropertyChanged and formatted properties
/// </summary>
public class RankItemViewModel : INotifyPropertyChanged
{
    private string _rankType = string.Empty;
    private int _rankValue;
    private int _progress;
    private string _rankName = string.Empty;

    public string RankType
    {
        get => _rankType;
        set
        {
            if (_rankType != value)
            {
                _rankType = value;
                OnPropertyChanged();
                UpdateRankName();
            }
        }
    }

    public int RankValue
    {
        get => _rankValue;
        set
        {
            if (_rankValue != value)
            {
                _rankValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RankFormatted));
                UpdateRankName();
            }
        }
    }

    public int Progress
    {
        get => _progress;
        set
        {
            if (_progress != value)
            {
                _progress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressFormatted));
            }
        }
    }

    public string RankName
    {
        get => _rankName;
        private set
        {
            if (_rankName != value)
            {
                _rankName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RankFormatted));
            }
        }
    }

    public string RankFormatted => $"{RankName} ({RankValue})";
    public string ProgressFormatted => $"{Progress}%";

    private void UpdateRankName()
    {
        RankName = RankNames.GetRankName(RankType, RankValue);
    }

    public void UpdateFrom(RankModel model)
    {
        RankValue = model.RankValue;
        Progress = model.Progress;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
