using SlevinthHeavenEliteDangerous.Services;
using Microsoft.UI.Dispatching;
using System;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for General control - manages UI state based on service events
/// </summary>
public sealed class GeneralViewModel : IDisposable
{
    private readonly FSDService _service;
    private readonly DispatcherQueue _dispatcherQueue;

    public FSDTimingViewModel FSDTiming { get; } = new();
    public FSDTargetViewModel FSDTarget { get; } = new();

    public GeneralViewModel(DispatcherQueue dispatcherQueue, FSDService service)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        // Subscribe to service events
        _service.TimingUpdated += OnTimingUpdated;
        _service.TargetUpdated += OnTargetUpdated;
        _service.DataLoaded += OnDataLoaded;
    }

    public void Dispose()
    {
        _service.TimingUpdated -= OnTimingUpdated;
        _service.TargetUpdated -= OnTargetUpdated;
        _service.DataLoaded -= OnDataLoaded;
    }

    private void OnTimingUpdated(object? sender, FSDTimingUpdatedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            FSDTiming.UpdateFromModel(e.Timing);
        });
    }

    private void OnTargetUpdated(object? sender, FSDTargetUpdatedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            FSDTarget.UpdateFromModel(e.Target);
        });
    }

    private void OnDataLoaded(object? sender, GeneralDataLoadedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            FSDTiming.UpdateFromModel(e.State.FSDTiming);
            FSDTarget.UpdateFromModel(e.State.FSDTarget);
        });
    }

}
