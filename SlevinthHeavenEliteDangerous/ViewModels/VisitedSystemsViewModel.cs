using SlevinthHeavenEliteDangerous.Services;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for VisitedSystems control - manages UI state based on service events
/// </summary>
public sealed class VisitedSystemsViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly VisitedSystemsService _service;
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly Dictionary<long, VisitedSystemCardViewModel> _systemVMs = [];
    private const int MaxSystemsToDisplay = 50;

    public ObservableCollection<VisitedSystemCardViewModel> VisitedSystems { get; } = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public VisitedSystemsViewModel(DispatcherQueue dispatcherQueue, VisitedSystemsService service)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        _service.SystemUIUpdateRequested += OnSystemUIUpdateRequested;
        _service.BodyUIUpdateRequested += OnBodyUIUpdateRequested;
        _service.DataLoaded += OnDataLoaded;

        VisitedSystems.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(VisitedSystems));
        };
    }

    public void Dispose()
    {
        _service.SystemUIUpdateRequested -= OnSystemUIUpdateRequested;
        _service.BodyUIUpdateRequested -= OnBodyUIUpdateRequested;
        _service.DataLoaded -= OnDataLoaded;
    }

    private void OnSystemUIUpdateRequested(object? sender, SystemUIUpdateEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[VisitedSystemsViewModel] OnSystemUIUpdateRequested: {e.System.StarSystem}, IsNew: {e.IsNew}");

        _dispatcherQueue.TryEnqueue(() =>
        {
            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsViewModel] UI thread: Processing {e.System.StarSystem}");

            if (e.IsNew)
            {
                var vm = new VisitedSystemCardViewModel(e.System);
                _systemVMs[e.System.SystemAddress] = vm;
                VisitedSystems.Insert(0, vm);

                while (VisitedSystems.Count > MaxSystemsToDisplay)
                    VisitedSystems.RemoveAt(VisitedSystems.Count - 1);

                System.Diagnostics.Debug.WriteLine($"[VisitedSystemsViewModel] Added new system to display. Total: {VisitedSystems.Count}");
            }
            else
            {
                if (!_systemVMs.TryGetValue(e.System.SystemAddress, out var vm)) return;

                if (e.NeedsTimestampNotification)
                    vm.NotifyLastVisitChanged();

                var displayIndex = VisitedSystems.IndexOf(vm);
                if (displayIndex > 0)
                {
                    VisitedSystems.RemoveAt(displayIndex);
                    VisitedSystems.Insert(0, vm);
                    System.Diagnostics.Debug.WriteLine($"[VisitedSystemsViewModel] Moved system to top. Index was: {displayIndex}");
                }
                else if (displayIndex < 0)
                {
                    VisitedSystems.Insert(0, vm);
                    while (VisitedSystems.Count > MaxSystemsToDisplay)
                        VisitedSystems.RemoveAt(VisitedSystems.Count - 1);
                    System.Diagnostics.Debug.WriteLine($"[VisitedSystemsViewModel] Inserted system at top (was beyond display limit)");
                }
            }
        });
    }

    private void OnBodyUIUpdateRequested(object? sender, BodyUIUpdateEventArgs e)
    {
        if (e.Body == null || e.System == null) return;

        _dispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                if (!_systemVMs.TryGetValue(e.System.SystemAddress, out var systemVM)) return;

                var bodyVM = systemVM.GetBodyVM(e.Body.BodyID);

                if (e.IsNewBody)
                {
                    System.Diagnostics.Debug.WriteLine($"[VisitedSystemsViewModel] Adding new body {e.Body.BodyName} to system {e.System.StarSystem}");

                    if (bodyVM == null)
                    {
                        bodyVM = new BodyCardViewModel(e.Body);
                        systemVM.RegisterBodyVM(bodyVM);
                    }

                    systemVM.OrganizeHierarchy();
                    System.Diagnostics.Debug.WriteLine($"[VisitedSystemsViewModel] Added body {e.Body.BodyName} (ID: {e.Body.BodyID}). Total: {systemVM.BodiesCount}");
                }

                if (bodyVM == null) return;

                if (e.ScanData != null)
                    bodyVM.ApplyScanData(e.ScanData);

                if (e.SignalData != null)
                    bodyVM.ApplySignalData(e.SignalData);

                if (e.ShouldMarkMapped)
                    bodyVM.MarkMapped();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VisitedSystemsViewModel] Exception in body update: {ex.Message}");
            }
        });
    }

    private void OnDataLoaded(object? sender, SystemsDataLoadedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            VisitedSystems.Clear();
            _systemVMs.Clear();
            _service.Clear();

            int displayCount = 0;
            foreach (var card in e.Systems.OrderByDescending(s => s.LastVisitTimestamp))
            {
                var vm = new VisitedSystemCardViewModel(card);

                // Notify signals for any body that has them
                foreach (var bodyVM in card.GetAllBodiesFlat()
                    .Select(b => vm.GetBodyVM(b.BodyID))
                    .Where(b => b != null && b.Signals.Count > 0))
                {
                    bodyVM!.NotifySignalsChanged();
                }

                vm.OrganizeHierarchy();

                _systemVMs[card.SystemAddress] = vm;
                _service.AllSystems.Add(card);
                _service.SystemsDict[card.SystemAddress] = card;

                if (displayCount < MaxSystemsToDisplay)
                {
                    VisitedSystems.Add(vm);
                    displayCount++;
                }
            }

            _service.FinishLoading();
        });
    }
}
