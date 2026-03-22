using SlevinthHeavenEliteDangerous.Services.Models;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel wrapping ExoBioSaleModel — owns computed and formatted properties
/// </summary>
public class ExoBioSaleViewModel
{
    private readonly ExoBioSaleModel _model;

    public ExoBioSaleViewModel(ExoBioSaleModel model)
    {
        _model = model;
        ItemsSold = model.ItemsSold.Select(i => new ExoBioSaleItemViewModel(i)).ToList();
    }

    public DateTime SaleTimestamp => _model.SaleTimestamp;
    public long MarketID => _model.MarketID;
    public string StationName => _model.StationName;
    public string SystemName => _model.SystemName;
    public long TotalValue => _model.TotalValue;
    public long TotalBonus => _model.TotalBonus;
    public long TotalEarnings => TotalValue + TotalBonus;
    public int ItemCount => _model.ItemsSold.Count;

    public string TotalValueFormatted => $"{TotalValue:N0} CR";
    public string TotalBonusFormatted => $"{TotalBonus:N0} CR";
    public string TotalEarningsFormatted => $"{TotalEarnings:N0} CR";

    public IReadOnlyList<ExoBioSaleItemViewModel> ItemsSold { get; }
}

/// <summary>
/// ViewModel wrapping ExoBioSaleItem — owns computed and formatted properties
/// </summary>
public class ExoBioSaleItemViewModel : INotifyPropertyChanged
{
    private readonly ExoBioSaleItem _model;
    private bool _isClusterHighlight;

    public ExoBioSaleItemViewModel(ExoBioSaleItem model)
    {
        _model = model;
    }

    public string Species_Localised => _model.Species_Localised;
    public string Species => _model.Species;
    public long Value => _model.Value;
    public long Bonus => _model.Bonus;
    public long Total => Value + Bonus;
    public string SystemName => _model.SystemName;
    public double DistanceFromSol => _model.DistanceFromSol;
    public double[]? StarPos => _model.StarPos;

    public bool IsClusterHighlight
    {
        get => _isClusterHighlight;
        set
        {
            if (_isClusterHighlight != value)
            {
                _isClusterHighlight = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ClusterHighlightVisibility));
            }
        }
    }

    public Visibility ClusterHighlightVisibility =>
        _isClusterHighlight ? Visibility.Visible : Visibility.Collapsed;

    public string ValueFormatted => $"{Value:N0} CR";
    public string BonusFormatted => $"{Bonus:N0} CR";
    public string TotalFormatted => $"{Total:N0} CR";
    public string DistanceFromSolFormatted => DistanceFromSol > 0 ? $"{DistanceFromSol:N2} ly" : string.Empty;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
