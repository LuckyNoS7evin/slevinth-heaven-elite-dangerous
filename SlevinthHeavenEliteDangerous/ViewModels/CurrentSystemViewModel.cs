using SlevinthHeavenEliteDangerous.Services;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SlevinthHeavenEliteDangerous.ViewModels;

/// <summary>
/// ViewModel for the current system display.
/// Shows only valuable bodies (ELW, WW, Terraformable, Landable with Bio).
/// </summary>
public class CurrentSystemViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly DispatcherQueue _dispatcherQueue;
    private readonly VisitedSystemsService _service;
    private VisitedSystemCardViewModel? _currentSystem;

    // Lookup of all system VMs seen during this session (for body event routing)
    private readonly Dictionary<long, VisitedSystemCardViewModel> _systemVMs = [];

    public ObservableCollection<BodyCardViewModel> ValuableBodies { get; } = [];

    public VisitedSystemCardViewModel? CurrentSystem
    {
        get => _currentSystem;
        private set
        {
            if (_currentSystem != value)
            {
                _currentSystem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasCurrentSystem));
                OnPropertyChanged(nameof(SystemName));
                OnPropertyChanged(nameof(ValuableBodiesCount));
            }
        }
    }

    public bool HasCurrentSystem => CurrentSystem != null;
    public string SystemName => CurrentSystem?.StarSystem ?? "Unknown";
    public string ValuableBodiesCount => $"{ValuableBodies.Count} valuable bodies";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public CurrentSystemViewModel(DispatcherQueue dispatcherQueue, VisitedSystemsService service)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        _service.SystemUIUpdateRequested += OnSystemUIUpdateRequested;
        _service.BodyUIUpdateRequested += OnBodyUIUpdateRequested;
        _service.DataLoaded += OnDataLoaded;
    }

    public void Dispose()
    {
        _service.SystemUIUpdateRequested -= OnSystemUIUpdateRequested;
        _service.BodyUIUpdateRequested -= OnBodyUIUpdateRequested;
        _service.DataLoaded -= OnDataLoaded;
    }

    private void OnDataLoaded(object? sender, SystemsDataLoadedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            _systemVMs.Clear();
            var mostRecent = e.Systems.OrderByDescending(s => s.LastVisitTimestamp).FirstOrDefault();
            if (mostRecent != null)
            {
                var vm = new VisitedSystemCardViewModel(mostRecent);
                vm.OrganizeHierarchy();
                _systemVMs[mostRecent.SystemAddress] = vm;
                CurrentSystem = vm;
                UpdateValuableBodies();
            }
        });
    }

    private void OnSystemUIUpdateRequested(object? sender, SystemUIUpdateEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            System.Diagnostics.Debug.WriteLine($"[CurrentSystemViewModel] System update: {e.System.StarSystem}, IsNew: {e.IsNew}");

            if (!_systemVMs.TryGetValue(e.System.SystemAddress, out var vm))
            {
                vm = new VisitedSystemCardViewModel(e.System);
                _systemVMs[e.System.SystemAddress] = vm;
            }

            CurrentSystem = vm;

            ValuableBodies.Clear();
            if (CurrentSystem != null)
                UpdateValuableBodies();
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

                if (bodyVM == null)
                {
                    if (!e.IsNewBody) return;
                    bodyVM = new BodyCardViewModel(e.Body);
                    systemVM.RegisterBodyVM(bodyVM);
                }

                if (e.ScanData != null)
                    bodyVM.ApplyScanData(e.ScanData);

                if (e.SignalData != null)
                    bodyVM.ApplySignalData(e.SignalData);

                if (e.ShouldMarkMapped)
                    bodyVM.MarkMapped();

                // Only update valuable bodies display for the current system
                if (CurrentSystem?.SystemAddress == e.System.SystemAddress)
                {
                    System.Diagnostics.Debug.WriteLine($"[CurrentSystemViewModel] Body matches current system");

                    if (IsValuableBody(bodyVM) && !ValuableBodies.Any(x => x.BodyID == bodyVM.BodyID))
                    {
                        System.Diagnostics.Debug.WriteLine($"[CurrentSystemViewModel] Adding valuable body: {bodyVM.BodyName}");
                        ValuableBodies.Add(bodyVM);
                        OnPropertyChanged(nameof(ValuableBodiesCount));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CurrentSystemViewModel] Exception in body update: {ex.Message}");
            }
        });
    }

    private void UpdateValuableBodies()
    {
        if (CurrentSystem == null) return;

        ValuableBodies.Clear();

        var allBodies = CurrentSystem.Bodies.ToList();
        var childBodies = new List<BodyCardViewModel>();
        foreach (var body in allBodies)
            CollectChildren(body, childBodies);
        allBodies.AddRange(childBodies);

        foreach (var body in allBodies)
        {
            if (IsValuableBody(body))
                ValuableBodies.Add(body);
        }

        OnPropertyChanged(nameof(ValuableBodiesCount));
    }

    private void CollectChildren(BodyCardViewModel parent, List<BodyCardViewModel> collection)
    {
        foreach (var child in parent.Children)
        {
            collection.Add(child);
            CollectChildren(child, collection);
        }
    }

    private bool IsValuableBody(BodyCardViewModel body)
    {
        System.Diagnostics.Debug.WriteLine($"[CurrentSystemViewModel] Checking if valuable: {body.BodyName}, Class: {body.PlanetClass}, Terraform: {body.TerraformState}, Landable: {body.Landable}, Signals: {body.Signals.Count}");

        if (BodyValueHelper.IsEarthLikeWorld(body.PlanetClass))
        {
            System.Diagnostics.Debug.WriteLine($"[CurrentSystemViewModel] -> Valuable: Earth Like World");
            return true;
        }

        if (BodyValueHelper.IsWaterWorld(body.PlanetClass))
        {
            System.Diagnostics.Debug.WriteLine($"[CurrentSystemViewModel] -> Valuable: Water World");
            return true;
        }

        if (BodyValueHelper.HasTerraformState(body.TerraformState))
        {
            System.Diagnostics.Debug.WriteLine($"[CurrentSystemViewModel] -> Valuable: Terraformable");
            return true;
        }

        if (body.Landable && body.HasBiologicalSignals)
        {
            System.Diagnostics.Debug.WriteLine($"[CurrentSystemViewModel] -> Valuable: Landable with Bio");
            return true;
        }

        System.Diagnostics.Debug.WriteLine($"[CurrentSystemViewModel] -> Not valuable");
        return false;
    }
}
