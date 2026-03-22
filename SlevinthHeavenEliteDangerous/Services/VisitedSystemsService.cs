using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SlevinthHeavenEliteDangerous.Services.Models;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Manages visited systems state, handles journal events, and persists data.
/// Merged from VisitedSystemsManager + VisitedSystemsService.
/// </summary>
public sealed class VisitedSystemsService : IEventHandler, IDisposable
{
    private readonly VisitedSystemsDataService _dataService = new();
    private DateTime _lastSaveRequest = DateTime.MinValue;
    private readonly TimeSpan _saveDebounceDelay = TimeSpan.FromMilliseconds(500);
    private Task? _saveTask;
    private readonly object _saveLock = new();
    private bool _isLoading = false;
    private bool _isBulkLoading = false;

    // --- State ---

    /// <summary>All visited systems (for saving and cross-control access).</summary>
    public List<VisitedSystemCard> AllSystems { get; } = [];

    /// <summary>Dictionary for fast lookup by SystemAddress.</summary>
    public Dictionary<long, VisitedSystemCard> SystemsDict { get; } = [];

    // --- Events ---

    /// <summary>Raised when systems data is loaded from disk.</summary>
    public event EventHandler<SystemsDataLoadedEventArgs>? DataLoaded;

    /// <summary>Raised when a system needs to be displayed/updated in UI.</summary>
    public event EventHandler<SystemUIUpdateEventArgs>? SystemUIUpdateRequested;

    /// <summary>Raised when a body needs to be added/updated in UI.</summary>
    public event EventHandler<BodyUIUpdateEventArgs>? BodyUIUpdateRequested;

    // --- State methods ---

    public VisitedSystemCard? GetSystem(long systemAddress) =>
        SystemsDict.TryGetValue(systemAddress, out var system) ? system : null;

    public IEnumerable<VisitedSystemCard> GetSystemsOrderedByLastVisit() =>
        AllSystems.OrderByDescending(s => s.LastVisitTimestamp);

    public void AddOrUpdateSystem(VisitedSystemCard system)
    {
        if (SystemsDict.ContainsKey(system.SystemAddress))
        {
            AllSystems.Remove(system);
            AllSystems.Insert(0, system);
        }
        else
        {
            AllSystems.Insert(0, system);
            SystemsDict[system.SystemAddress] = system;
        }
    }

    public void Clear()
    {
        AllSystems.Clear();
        SystemsDict.Clear();
    }

    /// <summary>
    /// Called from the UI thread after OnDataLoaded has fully repopulated AllSystems.
    /// Clears the loading guard so saves are no longer blocked.
    /// </summary>
    public void FinishLoading() => _isLoading = false;

    /// <summary>Suppresses UI events and saves during a bulk journal scan.</summary>
    public void BeginBulkLoad()
    {
        _isBulkLoading = true;
        _isLoading = true;
    }

    /// <summary>
    /// Ends bulk load mode, fires DataLoaded with a snapshot of accumulated state,
    /// and schedules a save. The ViewModel's OnDataLoaded handler calls FinishLoading()
    /// to clear _isLoading after it repopulates.
    /// </summary>
    public void EndBulkLoad()
    {
        _isBulkLoading = false;
        DataLoaded?.Invoke(this, new SystemsDataLoadedEventArgs(AllSystems.ToList()));
        ScheduleSave();
    }

    // --- Persistence ---

    public async Task LoadDataAsync()
    {
        _isLoading = true;
        try
        {
            var data = await _dataService.LoadDataAsync();
            if (data != null)
                DataLoaded?.Invoke(this, new SystemsDataLoadedEventArgs(data));
            else
                _isLoading = false;
        }
        catch
        {
            _isLoading = false;
            throw;
        }
    }

    private void ScheduleSave()
    {
        lock (_saveLock)
        {
            _lastSaveRequest = DateTime.UtcNow;
            if (_saveTask == null || _saveTask.IsCompleted)
                _saveTask = DebouncedSaveAsync();
        }
    }

    private async Task DebouncedSaveAsync()
    {
        while (true)
        {
            DateTime requestTime;
            lock (_saveLock) { requestTime = _lastSaveRequest; }

            var remaining = _saveDebounceDelay - (DateTime.UtcNow - requestTime);
            if (remaining > TimeSpan.Zero)
                await Task.Delay(remaining);
            else
            {
                lock (_saveLock)
                {
                    if (DateTime.UtcNow - _lastSaveRequest >= _saveDebounceDelay)
                        break;
                }
            }
        }

        try
        {
            if (!_isLoading)
                await _dataService.SaveDataAsync(AllSystems);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving visited systems data: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_dataService is IDisposable disposable)
            disposable.Dispose();
    }

    // --- Event handling ---

    public void HandleEvent(EventBase evt)
    {
        if (evt is FSDJumpEvent fsdJump)
            HandleFSDJumpEvent(fsdJump);
        else if (evt is ScanEvent scanEvent)
            HandleScanEvent(scanEvent);
        else if (evt is FSSBodySignalsEvent signalsEvent)
            HandleFSSBodySignalsEvent(signalsEvent);
        else if (evt is SAAScanCompleteEvent saaScanComplete)
            HandleSAAScanCompleteEvent(saaScanComplete);
    }

    private void HandleFSDJumpEvent(FSDJumpEvent evt)
    {
        System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] HandleFSDJumpEvent: {evt.StarSystem} (Address: {evt.SystemAddress})");

        if (!evt.SystemAddress.HasValue)
        {
            System.Diagnostics.Debug.WriteLine("[VisitedSystemsService] No SystemAddress - skipping");
            return;
        }

        long systemAddress = evt.SystemAddress.Value;
        var existingCard = GetSystem(systemAddress);

        if (existingCard != null)
        {
            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Revisit: {evt.StarSystem}");

            existingCard.LastVisitTimestamp = evt.Timestamp;

            AllSystems.Remove(existingCard);
            AllSystems.Insert(0, existingCard);

            if (!_isBulkLoading)
            {
                SystemUIUpdateRequested?.Invoke(this, new SystemUIUpdateEventArgs(existingCard, false, true));
                System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] SystemUIUpdateRequested raised (revisit)");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] New system: {evt.StarSystem}");

            var card = new VisitedSystemCard
            {
                SystemAddress = systemAddress,
                StarSystem = evt.StarSystem,
                StarPos = evt.StarPos,
                FirstVisitTimestamp = evt.Timestamp,
                LastVisitTimestamp = evt.Timestamp,
                DistanceFromSol = evt.StarPos != null
                    ? Math.Sqrt(Math.Pow(evt.StarPos[0], 2) + Math.Pow(evt.StarPos[1], 2) + Math.Pow(evt.StarPos[2], 2))
                    : 0
            };

            AddOrUpdateSystem(card);

            if (!_isBulkLoading)
            {
                SystemUIUpdateRequested?.Invoke(this, new SystemUIUpdateEventArgs(card, true, false));
                System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] SystemUIUpdateRequested raised (new system)");
            }
        }
        ScheduleSave();
    }

    private void HandleScanEvent(ScanEvent evt)
    {
        if (evt.ScanType != "Detailed") return;
        if (!string.IsNullOrWhiteSpace(evt.StarType)) return;
        if (!evt.SystemAddress.HasValue || !evt.BodyID.HasValue) return;

        long systemAddress = evt.SystemAddress.Value;
        var existingCard = GetSystem(systemAddress);

        if (existingCard != null)
        {
            var existingBody = existingCard.GetBodyByID(evt.BodyID.Value);
            BodyScanData? scanData = null;

            if (existingBody != null)
            {
                System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Updating existing body: {evt.BodyName} (ID: {evt.BodyID})");

                existingBody.BodyName = evt.BodyName;
                existingBody.WasDiscovered = evt.WasDiscovered ?? false;
                existingBody.WasMapped = evt.WasMapped ?? false;
                existingBody.WasFootfalled = evt.WasFootfalled ?? false;
                existingBody.Landable = evt.Landable ?? false;
                existingBody.TerraformState = evt.TerraformState;
                existingBody.PlanetClass = evt.PlanetClass;
                existingBody.DistanceFromArrivalLS = evt.DistanceFromArrivalLS ?? 0;

                int? planetParentID = null;
                int? starParentID = null;
                if (evt.Parents != null && evt.Parents.Count > 0)
                {
                    foreach (var parent in evt.Parents)
                    {
                        if (parent.Type.Equals("Planet", StringComparison.OrdinalIgnoreCase))
                            planetParentID = (int?)parent.Id;
                        else if (parent.Type.Equals("Star", StringComparison.OrdinalIgnoreCase))
                            starParentID = (int?)parent.Id;
                    }
                    existingBody.PlanetParentID = planetParentID;
                    existingBody.StarParentID = starParentID;
                }

                scanData = new BodyScanData(
                    BodyName: evt.BodyName,
                    WasDiscovered: evt.WasDiscovered ?? false,
                    WasMapped: evt.WasMapped ?? false,
                    WasFootfalled: evt.WasFootfalled ?? false,
                    Landable: evt.Landable ?? false,
                    TerraformState: evt.TerraformState,
                    PlanetClass: evt.PlanetClass,
                    DistanceFromArrivalLS: evt.DistanceFromArrivalLS ?? 0,
                    PlanetParentID: planetParentID,
                    StarParentID: starParentID
                );

                System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Updated existing body {evt.BodyName}: Planet: {scanData.PlanetParentID}, Star: {scanData.StarParentID}");

                if (!_isBulkLoading)
                {
                    System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Raising BodyUIUpdateRequested for EXISTING body: {existingBody.BodyName}");
                    BodyUIUpdateRequested?.Invoke(this, new BodyUIUpdateEventArgs(existingCard, existingBody, scanData: scanData));
                }
                ScheduleSave();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Creating new body: {evt.BodyName} (ID: {evt.BodyID})");

                var bodyCard = new BodyCard
                {
                    BodyName = evt.BodyName,
                    BodyID = evt.BodyID.Value,
                    WasDiscovered = evt.WasDiscovered ?? false,
                    WasMapped = evt.WasMapped ?? false,
                    WasFootfalled = evt.WasFootfalled ?? false,
                    Landable = evt.Landable ?? false,
                    TerraformState = evt.TerraformState,
                    PlanetClass = evt.PlanetClass,
                    DistanceFromArrivalLS = evt.DistanceFromArrivalLS ?? 0
                };

                if (evt.Parents != null && evt.Parents.Count > 0)
                {
                    foreach (var parent in evt.Parents)
                    {
                        if (parent.Type.Equals("Planet", StringComparison.OrdinalIgnoreCase))
                            bodyCard.PlanetParentID = (int?)parent.Id;
                        else if (parent.Type.Equals("Star", StringComparison.OrdinalIgnoreCase))
                            bodyCard.StarParentID = (int?)parent.Id;
                    }
                }

                existingCard.RegisterBody(bodyCard);
                System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Registered body {evt.BodyName} (ID: {evt.BodyID}) - Planet: {bodyCard.PlanetParentID}, Star: {bodyCard.StarParentID}");

                if (!_isBulkLoading)
                {
                    System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Raising BodyUIUpdateRequested for NEW body: {evt.BodyName}");
                    BodyUIUpdateRequested?.Invoke(this, new BodyUIUpdateEventArgs(existingCard, bodyCard, isNewBody: true));
                }
                ScheduleSave();
            }
        }
    }

    private void HandleFSSBodySignalsEvent(FSSBodySignalsEvent evt)
    {
        if (!evt.SystemAddress.HasValue || !evt.BodyID.HasValue) return;

        long systemAddress = evt.SystemAddress.Value;
        var existingCard = GetSystem(systemAddress);

        if (existingCard != null)
        {
            var existingBody = existingCard.GetBodyByID(evt.BodyID.Value);

            if (existingBody != null)
            {
                var signalData = new List<SignalCard>();
                foreach (var signal in evt.Signals)
                {
                    signalData.Add(new SignalCard
                    {
                        Type_Localised = signal.Type_Localised,
                        Count = signal.Count ?? 0
                    });
                }

                if (!_isBulkLoading)
                {
                    BodyUIUpdateRequested?.Invoke(this, new BodyUIUpdateEventArgs(existingCard, existingBody, signalData: signalData));
                }
                ScheduleSave();
            }
            else
            {
                var bodyCard = new BodyCard
                {
                    BodyName = evt.BodyName,
                    BodyID = evt.BodyID.Value,
                    WasDiscovered = false,
                    WasMapped = false,
                    WasFootfalled = false,
                    PlanetClass = string.Empty
                };

                foreach (var signal in evt.Signals)
                {
                    bodyCard.Signals.Add(new SignalCard
                    {
                        Type_Localised = signal.Type_Localised,
                        Count = signal.Count ?? 0
                    });
                }

                existingCard.RegisterBody(bodyCard);
                System.Diagnostics.Debug.WriteLine($"Registered body {evt.BodyName} (ID: {evt.BodyID}) with signals for system");

                if (!_isBulkLoading)
                {
                    BodyUIUpdateRequested?.Invoke(this, new BodyUIUpdateEventArgs(existingCard, bodyCard, isNewBody: true));
                }
                ScheduleSave();
            }
        }
    }

    private void HandleSAAScanCompleteEvent(SAAScanCompleteEvent evt)
    {
        if (!evt.SystemAddress.HasValue || !evt.BodyID.HasValue) return;

        try
        {
            long systemAddress = evt.SystemAddress.Value;
            var existingCard = GetSystem(systemAddress);

            if (existingCard != null)
            {
                var existingBody = existingCard.GetBodyByID(evt.BodyID.Value);

                if (existingBody != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Body {existingBody.BodyName} (ID: {evt.BodyID}) needs to be marked as mapped in system {existingCard.StarSystem}");

                    if (!_isBulkLoading)
                    {
                        try
                        {
                            BodyUIUpdateRequested?.Invoke(this, new BodyUIUpdateEventArgs(existingCard, existingBody, isNewBody: false, shouldMarkMapped: true));
                            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] BodyUIUpdateRequested event raised successfully");
                        }
                        catch (System.Runtime.InteropServices.COMException comEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] COMException while raising UI event: {comEx.Message}");
                            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] HRESULT: 0x{comEx.HResult:X8}");
                        }
                    }
                    ScheduleSave();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] WARNING: SAAScanComplete for unknown body ID {evt.BodyID} in system {existingCard.StarSystem}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] WARNING: SAAScanComplete for unknown system address {evt.SystemAddress}");
            }
        }
        catch (System.Runtime.InteropServices.COMException comEx)
        {
            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] COMException in HandleSAAScanCompleteEvent: {comEx.Message}");
            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] HRESULT: 0x{comEx.HResult:X8}");
            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Stack trace: {comEx.StackTrace}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Unexpected exception in HandleSAAScanCompleteEvent: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Exception type: {ex.GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"[VisitedSystemsService] Stack trace: {ex.StackTrace}");
        }
    }
}

/// <summary>Event args for systems data loaded events.</summary>
public class SystemsDataLoadedEventArgs : EventArgs
{
    public List<VisitedSystemCard> Systems { get; }
    public SystemsDataLoadedEventArgs(List<VisitedSystemCard> systems) => Systems = systems;
}

/// <summary>Event args for system UI update events.</summary>
public class SystemUIUpdateEventArgs : EventArgs
{
    public VisitedSystemCard System { get; }
    public bool IsNew { get; }
    public bool NeedsTimestampNotification { get; }

    public SystemUIUpdateEventArgs(VisitedSystemCard system, bool isNew, bool needsTimestampNotification)
    {
        System = system;
        IsNew = isNew;
        NeedsTimestampNotification = needsTimestampNotification;
    }
}

/// <summary>Event args for body UI update events.</summary>
public class BodyUIUpdateEventArgs : EventArgs
{
    public VisitedSystemCard System { get; }
    public BodyCard? Body { get; }
    public bool IsNewBody { get; }
    public bool ShouldMarkMapped { get; }
    public BodyScanData? ScanData { get; }
    public List<SignalCard>? SignalData { get; }

    public BodyUIUpdateEventArgs(VisitedSystemCard system, BodyCard? body, bool isNewBody = false, bool shouldMarkMapped = false, BodyScanData? scanData = null, List<SignalCard>? signalData = null)
    {
        System = system;
        Body = body;
        IsNewBody = isNewBody;
        ShouldMarkMapped = shouldMarkMapped;
        ScanData = scanData;
        SignalData = signalData;
    }
}

/// <summary>Immutable scan data passed from service to UI thread for deferred application.</summary>
public record BodyScanData(
    string? BodyName,
    bool WasDiscovered,
    bool WasMapped,
    bool WasFootfalled,
    bool Landable,
    string? TerraformState,
    string? PlanetClass,
    double DistanceFromArrivalLS,
    int? PlanetParentID,
    int? StarParentID
);
