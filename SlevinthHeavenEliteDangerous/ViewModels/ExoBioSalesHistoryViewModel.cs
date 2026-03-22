using SlevinthHeavenEliteDangerous.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for ExoBio sales history
/// </summary>
public class ExoBioSalesHistoryViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly ExoBioService _service;

    public ObservableCollection<ExoBioSaleViewModel> Sales { get; } = [];

    public string TotalSalesFormatted => $"{Sales.Count} sales • {TotalEarnings:N0} CR";

    public long TotalEarnings => Sales.Sum(s => s.TotalEarnings);

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ExoBioSalesHistoryViewModel(DispatcherQueue dispatcherQueue, ExoBioService service)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        _service.SaleAdded += OnSaleAdded;
    }

    public void Dispose()
    {
        _service.SaleAdded -= OnSaleAdded;
    }

    private void OnSaleAdded(object? sender, ExoBioSaleEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            Sales.Insert(0, new ExoBioSaleViewModel(e.Sale));
            ComputeClusterHighlights();
            OnPropertyChanged(nameof(TotalSalesFormatted));
            OnPropertyChanged(nameof(TotalEarnings));
        });
    }

    private void ComputeClusterHighlights()
    {
        var allItems = Sales.SelectMany(s => s.ItemsSold).ToList();

        foreach (var item in allItems)
        {
            if (item.Value <= 7_000_000 || item.StarPos == null)
            {
                item.IsClusterHighlight = false;
                continue;
            }

            item.IsClusterHighlight = allItems.Any(other =>
                !ReferenceEquals(other, item) &&
                !string.Equals(other.SystemName, item.SystemName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(other.Species, item.Species, StringComparison.OrdinalIgnoreCase) &&
                other.StarPos != null &&
                SystemDistance(item.StarPos, other.StarPos) <= 100.0);
        }
    }

    private static double SystemDistance(double[] a, double[] b)
        => Math.Sqrt(Math.Pow(a[0] - b[0], 2) + Math.Pow(a[1] - b[1], 2) + Math.Pow(a[2] - b[2], 2));
}
