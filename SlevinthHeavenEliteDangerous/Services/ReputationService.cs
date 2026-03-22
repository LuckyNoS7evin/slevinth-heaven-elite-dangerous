using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Events;
using SlevinthHeavenEliteDangerous.Services.Models;
using System;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Tracks commander faction reputation from journal events.
/// </summary>
public class ReputationService : IEventHandler
{
    private readonly ReputationDataService _dataService = new();
    private readonly ReputationModel _reputation = new();
    private bool _isLoading;

    public event EventHandler<ReputationUpdatedEventArgs>? ReputationUpdated;

    public void HandleEvent(EventBase evt)
    {
        if (evt is ReputationEvent rep)
            HandleReputationEvent(rep);
    }

    private void HandleReputationEvent(ReputationEvent evt)
    {
        System.Diagnostics.Debug.WriteLine(
            $"[ReputationService] Empire={evt.Empire:F1} Federation={evt.Federation:F1} " +
            $"Independent={evt.Independent:F1} Alliance={evt.Alliance:F1}");

        _reputation.Empire      = evt.Empire;
        _reputation.Federation  = evt.Federation;
        _reputation.Independent = evt.Independent;
        _reputation.Alliance    = evt.Alliance;

        ReputationUpdated?.Invoke(this, new ReputationUpdatedEventArgs(_reputation));
        ScheduleSave();
    }

    public ReputationModel GetReputation() => _reputation;

    public async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            var data = await _dataService.LoadDataAsync();
            if (data != null)
            {
                _reputation.Empire      = data.Empire;
                _reputation.Federation  = data.Federation;
                _reputation.Independent = data.Independent;
                _reputation.Alliance    = data.Alliance;

                System.Diagnostics.Debug.WriteLine("[ReputationService] Loaded reputation data.");
                ReputationUpdated?.Invoke(this, new ReputationUpdatedEventArgs(_reputation));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ReputationService] No existing reputation data, starting fresh.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReputationService] Error loading data: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void ScheduleSave()
    {
        if (_isLoading) return;

        try
        {
            await _dataService.SaveDataAsync(new ReputationModel
            {
                Empire      = _reputation.Empire,
                Federation  = _reputation.Federation,
                Independent = _reputation.Independent,
                Alliance    = _reputation.Alliance
            });
            System.Diagnostics.Debug.WriteLine("[ReputationService] Save completed.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ReputationService] Error saving: {ex.Message}");
        }
    }
}

public class ReputationUpdatedEventArgs : EventArgs
{
    public ReputationModel Reputation { get; }

    public ReputationUpdatedEventArgs(ReputationModel reputation)
    {
        Reputation = reputation;
    }
}
