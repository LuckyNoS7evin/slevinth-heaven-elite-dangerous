using SlevinthHeavenEliteDangerous.Services;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for ExoBio control - manages UI state based on service events
/// </summary>
public sealed class ExoBioViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly ExoBioService _service;
    private readonly DispatcherQueue _dispatcherQueue;
    private long _submittedTotal = 0;

    public ObservableCollection<ExoBioCardViewModel> ExoBioCards { get; } = [];

    public long SubmittedTotal
    {
        get => _submittedTotal;
        set
        {
            if (_submittedTotal != value)
            {
                _submittedTotal = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SubmittedTotalFormatted));
            }
        }
    }

    public string SubmittedTotalFormatted => SubmittedTotal.ToString("N0") + " CR";

    public long PossibleEarningsFD => ExoBioCards.Where(c => c.ScanType == "Analyse").Sum(c => c.EstimatedValue + c.EstimatedBonus);

    public string PossibleEarningsFDFormatted => PossibleEarningsFD.ToString("N0") + " CR";

    public long PossibleEarnings => ExoBioCards.Where(c => c.ScanType == "Analyse").Sum(c => c.EstimatedValue);

    public string PossibleEarningsFormatted => PossibleEarnings.ToString("N0") + " CR";

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ExoBioViewModel(DispatcherQueue dispatcherQueue, ExoBioService service)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        // Subscribe to service events
        _service.DiscoveryAdded += OnDiscoveryAdded;
        _service.DiscoveryUpdated += OnDiscoveryUpdated;
        _service.DiscoveriesSubmitted += OnDiscoveriesSubmitted;
        _service.DataLoaded += OnDataLoaded;
    }

    public void Dispose()
    {
        _service.DiscoveryAdded -= OnDiscoveryAdded;
        _service.DiscoveryUpdated -= OnDiscoveryUpdated;
        _service.DiscoveriesSubmitted -= OnDiscoveriesSubmitted;
        _service.DataLoaded -= OnDataLoaded;
    }

    private void OnDiscoveryAdded(object? sender, ExoBioDiscoveryEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            if (e.ShouldClearIncomplete)
            {
                var sampleCards = ExoBioCards.Where(card => card.ScanType == "Sample" || card.ScanType == "Log").ToList();
                foreach (var card in sampleCards)
                {
                    ExoBioCards.Remove(card);
                }
            }

            var cardViewModel = new ExoBioCardViewModel
            {
                Key = e.Discovery.Key,
                Title = e.Discovery.Title,
                Details = e.Discovery.Details,
                ScanType = e.Discovery.ScanType,
                SampleValue = e.Discovery.SampleValue,
                EstimatedValue = e.Discovery.EstimatedValue,
                EstimatedBonus = e.Discovery.EstimatedBonus
            };

            ExoBioCards.Add(cardViewModel);
            NotifyEarningsChanged();
        });
    }

    private void OnDiscoveryUpdated(object? sender, ExoBioDiscoveryEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var existingCard = ExoBioCards.FirstOrDefault(card => card.Key == e.Discovery.Key);
            if (existingCard != null)
            {
                existingCard.ScanType = e.Discovery.ScanType;
                existingCard.EstimatedValue = e.Discovery.EstimatedValue;
                existingCard.EstimatedBonus = e.Discovery.EstimatedBonus;
            }
            NotifyEarningsChanged();
        });
    }

    private void OnDiscoveriesSubmitted(object? sender, ExoBioSubmittedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            SubmittedTotal += e.TotalEarnings;
            ExoBioCards.Clear();
            NotifyEarningsChanged();
        });
    }

    private void OnDataLoaded(object? sender, ExoBioDataLoadedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            SubmittedTotal = e.State.SubmittedTotal;

            ExoBioCards.Clear();
            foreach (var discovery in e.State.Discoveries)
            {
                var cardViewModel = new ExoBioCardViewModel
                {
                    Key = discovery.Key,
                    Title = discovery.Title,
                    Details = discovery.Details,
                    ScanType = discovery.ScanType,
                    SampleValue = discovery.SampleValue,
                    EstimatedValue = discovery.EstimatedValue,
                    EstimatedBonus = discovery.EstimatedBonus
                };
                ExoBioCards.Add(cardViewModel);
            }
            NotifyEarningsChanged();
        });
    }

    private void NotifyEarningsChanged()
    {
        OnPropertyChanged(nameof(PossibleEarningsFD));
        OnPropertyChanged(nameof(PossibleEarningsFDFormatted));
        OnPropertyChanged(nameof(PossibleEarnings));
        OnPropertyChanged(nameof(PossibleEarningsFormatted));
    }
}
