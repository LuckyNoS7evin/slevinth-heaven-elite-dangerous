using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Services.Models;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Service for managing ExoBio discoveries and events
/// </summary>
public sealed class ExoBioService : IEventHandler, IDisposable
{
    private readonly ExoBioDataService _dataService = new();
    private readonly VisitedSystemsService _systemsManager;
    private readonly HashSet<string> _exoBioKeys = [];
    private readonly Dictionary<string, ExoBioDiscoveryModel> _discoveries = [];
    private readonly List<ExoBioSaleModel> _salesHistory = [];
    private long _submittedTotal = 0;
    private DateTime? _lastEventTime = null;
    private bool _isLoading = false;
    private DateTime _lastSaveRequest = DateTime.MinValue;
    private readonly TimeSpan _saveDebounceDelay = TimeSpan.FromMilliseconds(500);
    private Task? _saveTask;
    private readonly object _saveLock = new();

    public ExoBioService(VisitedSystemsService systemsManager)
    {
        _systemsManager = systemsManager ?? throw new ArgumentNullException(nameof(systemsManager));
    }

    public void Dispose()
    {
        if (_dataService is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Event raised when an ExoBio discovery is added
    /// </summary>
    public event EventHandler<ExoBioDiscoveryEventArgs>? DiscoveryAdded;

    /// <summary>
    /// Event raised when an ExoBio discovery is updated
    /// </summary>
    public event EventHandler<ExoBioDiscoveryEventArgs>? DiscoveryUpdated;

    /// <summary>
    /// Event raised when ExoBio discoveries are cleared after submission
    /// </summary>
    public event EventHandler<ExoBioSubmittedEventArgs>? DiscoveriesSubmitted;

    /// <summary>
    /// Event raised when a sale transaction is recorded
    /// </summary>
    public event EventHandler<ExoBioSaleEventArgs>? SaleAdded;

    /// <summary>
    /// Event raised when data is loaded
    /// </summary>
    public event EventHandler<ExoBioDataLoadedEventArgs>? DataLoaded;

    /// <summary>
    /// Handle incoming game events
    /// </summary>
    public void HandleEvent(EventBase evt)
    {
        if (evt is ScanOrganicEvent scanOrganic)
        {
            HandleScanOrganicEvent(scanOrganic);
        }
        else if (evt is SellOrganicDataEvent sellOrganic)
        {
            HandleSellOrganicDataEvent(sellOrganic);
        }
    }

    /// <summary>
    /// Load data from disk
    /// </summary>
    public async Task<ExoBioStateModel> LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            var data = await _dataService.LoadDataAsync();
            if (data != null)
            {
                _lastEventTime = data.LastEventTime;
                _submittedTotal = data.SubmittedTotal;

                _exoBioKeys.Clear();
                _discoveries.Clear();
                _salesHistory.Clear();

                foreach (var key in data.Keys)
                    _exoBioKeys.Add(key);

                foreach (var discovery in data.Discoveries)
                    _discoveries[discovery.Key] = discovery;

                foreach (var sale in data.Sales)
                {
                    _salesHistory.Add(sale);
                    SaleAdded?.Invoke(this, new ExoBioSaleEventArgs(sale));
                }

                var state = new ExoBioStateModel
                {
                    SubmittedTotal = _submittedTotal,
                    Discoveries = _discoveries.Values.ToList()
                };

                DataLoaded?.Invoke(this, new ExoBioDataLoadedEventArgs(state));
                return state;
            }

            return new ExoBioStateModel();
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Save current state to disk
    /// </summary>
    public void ScheduleSave()
    {
        lock (_saveLock)
        {
            _lastSaveRequest = DateTime.UtcNow;

            if (_saveTask == null || _saveTask.IsCompleted)
            {
                _saveTask = DebouncedSaveAsync();
            }
        }
    }

    private async Task DebouncedSaveAsync()
    {
        while (true)
        {
            DateTime saveRequestTime;
            lock (_saveLock)
            {
                saveRequestTime = _lastSaveRequest;
            }

            var elapsed = DateTime.UtcNow - saveRequestTime;
            var remaining = _saveDebounceDelay - elapsed;

            if (remaining > TimeSpan.Zero)
            {
                await Task.Delay(remaining);
            }
            else
            {
                lock (_saveLock)
                {
                    if (DateTime.UtcNow - _lastSaveRequest >= _saveDebounceDelay)
                    {
                        break;
                    }
                }
            }
        }

        try
        {
            await SaveDataAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving ExoBio data: {ex.Message}");
        }
    }

    private async Task SaveDataAsync()
    {
        if (_isLoading) return;

        var data = new ExoBioStateModel
        {
            LastEventTime = _lastEventTime,
            SubmittedTotal = _submittedTotal,
            Keys = [.. _exoBioKeys],
            Discoveries = [.. _discoveries.Values],
            Sales = [.. _salesHistory]
        };

        await _dataService.SaveDataAsync(data);
    }

    private void HandleScanOrganicEvent(ScanOrganicEvent evt)
    {
        System.Diagnostics.Debug.WriteLine($"Received ScanOrganicEvent: Genus={evt.Genus}, Species={evt.Species}, Variant={evt.Variant}, SystemAddress={evt.SystemAddress}, Body={evt.Body}, ScanType={evt.ScanType}, Timestamp={evt.Timestamp}");
        // Skip events that have already been processed
        if (_lastEventTime.HasValue && evt.Timestamp <= _lastEventTime.Value)
        {
            return;
        }

        _lastEventTime = evt.Timestamp;

        string key = $"{evt.Genus}|{evt.Variant}|{evt.SystemAddress}|{evt.Body}";
        bool isNewOrganism = !_exoBioKeys.Contains(key);

        if (isNewOrganism)
        {
            // Check if there are incomplete cards (Sample/Log that haven't been Analysed)
            bool hasIncompleteCards = _discoveries.Values.Any(d => 
                d.ScanType.Equals("Sample", StringComparison.OrdinalIgnoreCase) || 
                d.ScanType.Equals("Log", StringComparison.OrdinalIgnoreCase));

            if (string.Equals(evt.ScanType, "Sample", StringComparison.OrdinalIgnoreCase) || 
                string.Equals(evt.ScanType, "Log", StringComparison.OrdinalIgnoreCase))
            {
                // If there are incomplete cards, remove them from the service dictionary
                if (hasIncompleteCards)
                {
                    var incompleteKeys = _discoveries
                        .Where(kvp => kvp.Value.ScanType.Equals("Sample", StringComparison.OrdinalIgnoreCase) || 
                                     kvp.Value.ScanType.Equals("Log", StringComparison.OrdinalIgnoreCase))
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var incompleteKey in incompleteKeys)
                    {
                        _discoveries.Remove(incompleteKey);
                        _exoBioKeys.Remove(incompleteKey);
                    }

                    System.Diagnostics.Debug.WriteLine($"Removed {incompleteKeys.Count} incomplete cards before adding new discovery");
                }

                _exoBioKeys.Add(key);

                var existingCard = _systemsManager.GetSystem(evt.SystemAddress ?? 0);
                var systemName = $"{evt.SystemAddress}";
                var bodyName = $"{evt.Body}";
                if (existingCard != null)
                {
                    systemName = existingCard.StarSystem ?? $"{evt.SystemAddress}";
                    // Use GetBodyByID instead of accessing Bodies collection directly (thread-safe)
                    if (evt.Body.HasValue)
                    {
                        var body = existingCard.GetBodyByID(evt.Body.Value);
                        bodyName = body?.BodyName ?? $"{evt.Body}";
                    }
                }

                long sampleValue = ExobiologyValues.GetValue(evt.Species_Localised);

                var discovery = new ExoBioDiscoveryModel
                {
                    Key = key,
                    Title = evt.Genus_Localised ?? evt.Genus,
                    Details = $"Variant: {evt.Variant_Localised ?? evt.Variant}\n" +
                              $"Species: {evt.Species_Localised ?? evt.Species}\n" +
                              $"System: {systemName}\n" +
                              $"Body: {bodyName}\n" +
                              $"First Logged: {evt.Timestamp:yyyy-MM-dd HH:mm:ss}",
                    ScanType = evt.ScanType,
                    SampleValue = sampleValue,
                    EstimatedValue = 0,
                    EstimatedBonus = 0,
                    SystemName = systemName,
                    DistanceFromSol = existingCard?.DistanceFromSol ?? 0,
                    StarPos = existingCard?.StarPos
                };

                // Register discovery in service dictionary for data integrity
                _discoveries[key] = discovery;

                DiscoveryAdded?.Invoke(this, new ExoBioDiscoveryEventArgs(discovery, hasIncompleteCards));
                ScheduleSave();
            }
        }
        else
        {
            if (string.Equals(evt.ScanType, "Analyse", StringComparison.OrdinalIgnoreCase))
            {
                long estimatedValue = ExobiologyValues.GetValue(evt.Species_Localised);
                long estimatedBonus = ExobiologyValues.CalculateFirstDiscoveryBonus(estimatedValue);

                // Update existing discovery in service dictionary
                if (_discoveries.TryGetValue(key, out var existingDiscovery))
                {
                    existingDiscovery.ScanType = "Analyse";
                    existingDiscovery.EstimatedValue = estimatedValue;
                    existingDiscovery.EstimatedBonus = estimatedBonus;
                }

                var discovery = new ExoBioDiscoveryModel
                {
                    Key = key,
                    ScanType = "Analyse",
                    EstimatedValue = estimatedValue,
                    EstimatedBonus = estimatedBonus
                };

                DiscoveryUpdated?.Invoke(this, new ExoBioDiscoveryEventArgs(discovery, false));
                ScheduleSave();
            }
            else if (string.Equals(evt.ScanType, "Sample", StringComparison.OrdinalIgnoreCase))
            {
                // Update existing discovery scan type from Log to Sample
                if (_discoveries.TryGetValue(key, out var existingDiscovery))
                {
                    existingDiscovery.ScanType = "Sample";
                }

                var discovery = new ExoBioDiscoveryModel
                {
                    Key = key,
                    ScanType = "Sample"
                };

                DiscoveryUpdated?.Invoke(this, new ExoBioDiscoveryEventArgs(discovery, false));
                ScheduleSave();
            }
        }
    }

    private void HandleSellOrganicDataEvent(SellOrganicDataEvent evt)
    {
        _lastEventTime = evt.Timestamp;

        long totalEarnings = evt.BioData.Sum(data => data.Value + data.Bonus);
        _submittedTotal += totalEarnings;

        // Build a per-Genus|Variant queue so we can match each sold item to its source discovery
        var discoveryQueue = _discoveries.Values
            .GroupBy(d => $"{d.Key.Split('|')[0]}|{d.Key.Split('|')[1]}")
            .ToDictionary(g => g.Key, g => new Queue<ExoBioDiscoveryModel>(g));

        // Create sale record with details
        var sale = new ExoBioSaleModel
        {
            SaleTimestamp = evt.Timestamp,
            MarketID = evt.MarketID,
            TotalValue = evt.BioData.Sum(data => data.Value),
            TotalBonus = evt.BioData.Sum(data => data.Bonus),
            ItemsSold = evt.BioData.Select(data =>
            {
                string lookupKey = $"{data.Genus}|{data.Variant}";
                ExoBioDiscoveryModel? match = null;
                if (discoveryQueue.TryGetValue(lookupKey, out var queue) && queue.Count > 0)
                    match = queue.Dequeue();

                return new ExoBioSaleItem
                {
                    Species_Localised = data.Species_Localised,
                    Species = data.Species,
                    Value = data.Value,
                    Bonus = data.Bonus,
                    SystemName = match?.SystemName ?? string.Empty,
                    DistanceFromSol = match?.DistanceFromSol ?? 0,
                    StarPos = match?.StarPos
                };
            }).ToList()
        };

        _salesHistory.Add(sale);

        // Clear current discoveries
        _exoBioKeys.Clear();
        _discoveries.Clear();

        // Raise events
        SaleAdded?.Invoke(this, new ExoBioSaleEventArgs(sale));
        DiscoveriesSubmitted?.Invoke(this, new ExoBioSubmittedEventArgs(totalEarnings));
        ScheduleSave();
    }
}

/// <summary>
/// Event args for ExoBio discovery events
/// </summary>
public class ExoBioDiscoveryEventArgs : EventArgs
{
    public ExoBioDiscoveryModel Discovery { get; }
    public bool ShouldClearIncomplete { get; }

    public ExoBioDiscoveryEventArgs(ExoBioDiscoveryModel discovery, bool shouldClearIncomplete)
    {
        Discovery = discovery;
        ShouldClearIncomplete = shouldClearIncomplete;
    }
}

/// <summary>
/// Event args for ExoBio submission events
/// </summary>
public class ExoBioSubmittedEventArgs : EventArgs
{
    public long TotalEarnings { get; }

    public ExoBioSubmittedEventArgs(long totalEarnings)
    {
        TotalEarnings = totalEarnings;
    }
}

/// <summary>
/// Event args for ExoBio data loaded events
/// </summary>
public class ExoBioDataLoadedEventArgs : EventArgs
{
    public ExoBioStateModel State { get; }

    public ExoBioDataLoadedEventArgs(ExoBioStateModel state)
    {
        State = state;
    }
}

/// <summary>
/// Event args for ExoBio sale events
/// </summary>
public class ExoBioSaleEventArgs : EventArgs
{
    public ExoBioSaleModel Sale { get; }

    public ExoBioSaleEventArgs(ExoBioSaleModel sale)
    {
        Sale = sale;
    }
}
