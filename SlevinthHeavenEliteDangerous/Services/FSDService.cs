using SlevinthHeavenEliteDangerous.Events;
using System.Threading.Tasks;
using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Services.Models;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Service for managing FSD (Frame Shift Drive) related events and statistics
/// </summary>
public sealed class FSDService : IEventHandler, IDisposable
{
    private static readonly string JournalDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            @"Saved Games\Frontier Developments\Elite Dangerous");

    private readonly GeneralControlDataService _dataService = new();
    private FSDTimingModel _state = new();
    private string _currentFinalDestination = string.Empty;
    private bool _isLoading = false;
    private DateTime _lastSaveRequest = DateTime.MinValue;
    private readonly TimeSpan _saveDebounceDelay = TimeSpan.FromMilliseconds(500);
    private Task? _saveTask;
    private readonly object _saveLock = new();

    public FSDService()
    {
    }

    public void Dispose()
    {
        if (_dataService is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Event raised when FSD timing statistics are updated
    /// </summary>
    public event EventHandler<FSDTimingUpdatedEventArgs>? TimingUpdated;

    /// <summary>
    /// Event raised when FSD target information is updated
    /// </summary>
    public event EventHandler<FSDTargetUpdatedEventArgs>? TargetUpdated;

    /// <summary>
    /// Event raised when data is loaded
    /// </summary>
    public event EventHandler<GeneralDataLoadedEventArgs>? DataLoaded;

    /// <summary>
    /// Handle incoming game events
    /// </summary>
    public void HandleEvent(EventBase evt)
    {
        if (evt is FSDJumpEvent fsdJump)
        {
            HandleFSDJumpEvent(fsdJump);
        }
        else if (evt is FSDTargetEvent fsdTarget)
        {
            HandleFSDTargetEvent(fsdTarget);
        }
        else if (evt is NavRouteEvent)
        {
            HandleNavRouteEvent();
        }
        else if (evt is NavRouteClearEvent)
        {
            HandleNavRouteClearEvent();
        }
    }

    /// <summary>
    /// Load data from disk
    /// </summary>
    public async Task<GeneralStateModel> LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            var timing = await _dataService.LoadDataAsync();
            if (timing != null)
            {
                _state = timing;
                var state = new GeneralStateModel { FSDTiming = _state };
                DataLoaded?.Invoke(this, new GeneralDataLoadedEventArgs(state));
                return state;
            }

            return new GeneralStateModel();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void ScheduleSave(GeneralStateModel state)
    {
        lock (_saveLock)
        {
            _lastSaveRequest = DateTime.UtcNow;

            if (_saveTask == null || _saveTask.IsCompleted)
            {
                _saveTask = DebouncedSaveAsync(state);
            }
        }
    }

    private async Task DebouncedSaveAsync(GeneralStateModel state)
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
            await SaveDataAsync(state);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving FSD data: {ex.Message}");
        }
    }

    private async Task SaveDataAsync(GeneralStateModel state)
    {
        if (_isLoading) return;

        await _dataService.SaveDataAsync(state.FSDTiming);
    }

    private void HandleFSDJumpEvent(FSDJumpEvent evt)
    {
        if (_state.LastJumpTimestamp.HasValue)
        {
            var timeDiff = (evt.Timestamp - _state.LastJumpTimestamp.Value).TotalMilliseconds;

            // Ignore negative intervals (out-of-order timestamps) and any intervals > 24 hours
            if (timeDiff > 0 && timeDiff <= 86_400_000)
            {
                // Incremental average: (existingCount * oldAvg + newValue) / (existingCount + 1)
                // Interval count so far = TotalJumps - 1 (first jump sets timestamp, no interval yet)
                int allCount = _state.TotalJumps - 1;
                _state.AvgTimeAllJumps = (allCount * _state.AvgTimeAllJumps + timeDiff) / (allCount + 1);

                if (timeDiff < 120000)
                {
                    _state.AvgTimeFastJumps = (_state.FastJumpsCount * _state.AvgTimeFastJumps + timeDiff) / (_state.FastJumpsCount + 1);
                    _state.FastJumpsCount++;
                }

                if (_state.ShortestTime == 0 || timeDiff < _state.ShortestTime)
                    _state.ShortestTime = timeDiff;

                TimingUpdated?.Invoke(this, new FSDTimingUpdatedEventArgs(_state));
                ScheduleSave(new GeneralStateModel { FSDTiming = _state });
            }
        }

        _state.TotalJumps++;

        // Only update the last jump timestamp when the incoming event is newer than the stored value.
        // This prevents out-of-order events from moving the timestamp backwards and producing
        // negative interval values for subsequent jumps.
        if (!_state.LastJumpTimestamp.HasValue || evt.Timestamp > _state.LastJumpTimestamp.Value)
        {
            _state.LastJumpTimestamp = evt.Timestamp;
        }
    }

    private void HandleFSDTargetEvent(FSDTargetEvent evt)
    {
        var target = new FSDTargetModel
        {
            NextSystem = string.IsNullOrWhiteSpace(evt.Name) ? "No Target" : evt.Name,
            RemainingJumps = evt.RemainingJumpsInRoute ?? 0,
            FinalDestination = _currentFinalDestination
        };

        // Compute an estimated arrival time (UTC) using the average fast jump time
        // stored in _state.AvgTimeFastJumps (milliseconds). Only provide an estimate
        // when we have both a non-zero remaining jumps count and a recorded average.
        if (target.RemainingJumps > 0 && _state.AvgTimeFastJumps > 0)
        {
            try
            {
                var estimatedMs = target.RemainingJumps * _state.AvgTimeFastJumps;
                target.EstimatedArrivalUtc = DateTime.UtcNow.AddMilliseconds(estimatedMs);
            }
            catch
            {
                // On overflow or other errors, leave estimate as null.
                target.EstimatedArrivalUtc = null;
            }
        }

        TargetUpdated?.Invoke(this, new FSDTargetUpdatedEventArgs(target));
    }

    private void HandleNavRouteEvent()
    {
        var navRoutePath = Path.Combine(JournalDirectory, "NavRoute.json");
        if (!File.Exists(navRoutePath))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(navRoutePath);
            var routeData = JsonSerializer.Deserialize<NavRouteData>(json);
            var finalEntry = routeData?.Route?.LastOrDefault();
            _currentFinalDestination = finalEntry?.StarSystem ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FSDService] Failed to read NavRoute.json: {ex.Message}");
            _currentFinalDestination = string.Empty;
        }
    }

    private void HandleNavRouteClearEvent()
    {
        _currentFinalDestination = string.Empty;
        TargetUpdated?.Invoke(this, new FSDTargetUpdatedEventArgs(new FSDTargetModel()));
    }


}

/// <summary>
/// Minimal model for deserialising NavRoute.json
/// </summary>
file sealed class NavRouteData
{
    [JsonPropertyName("Route")]
    public NavRouteEntry[]? Route { get; set; }
}

file sealed class NavRouteEntry
{
    [JsonPropertyName("StarSystem")]
    public string? StarSystem { get; set; }
}

/// <summary>
/// Event args for FSD timing update events
/// </summary>
public class FSDTimingUpdatedEventArgs : EventArgs
{
    public FSDTimingModel Timing { get; }

    public FSDTimingUpdatedEventArgs(FSDTimingModel timing)
    {
        Timing = timing;
    }
}

/// <summary>
/// Event args for FSD target update events
/// </summary>
public class FSDTargetUpdatedEventArgs : EventArgs
{
    public FSDTargetModel Target { get; }

    public FSDTargetUpdatedEventArgs(FSDTargetModel target)
    {
        Target = target;
    }
}

/// <summary>
/// Event args for general data loaded events
/// </summary>
public class GeneralDataLoadedEventArgs : EventArgs
{
    public GeneralStateModel State { get; }

    public GeneralDataLoadedEventArgs(GeneralStateModel state)
    {
        State = state;
    }
}
