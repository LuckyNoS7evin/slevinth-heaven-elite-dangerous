using SlevinthHeavenEliteDangerous.VoCore;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using SlevinthHeavenEliteDangerous.Services.Models;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Service responsible for application startup operations
/// </summary>
public sealed class StartupService : IStartupService
{
    /// <summary>
    /// Increment this when a new field is added to an existing persisted model that requires
    /// journal history to populate. A mismatch with the saved schema_version.json will delete
    /// all data files and trigger a full journal rescan automatically.
    ///
    /// History:
    ///   1 — initial versioned baseline
    ///   2 — added SystemName + StarPos to ExoBioDiscoveryModel and ExoBioSaleItem for cluster detection
    ///   3 - adjusted the model above slightly
    /// </summary>
    private const int RequiredDataSchemaVersion = 3;

    private const string SchemaVersionFileName = "schema_version.json";
    private readonly JournalEventService _journalEventService;
    private readonly ExoBioService _exoBioService;
    private readonly VisitedSystemsService _visitedSystemsService;
    private readonly FSDService _fsdService;
    private readonly RankService _rankService;
    private readonly OverlayLogService _overlayLogService;
    private readonly CommanderStatsService _commanderStatsService;
    private readonly ReputationService _reputationService;
    private readonly CodexService _codexService;
    private readonly CombatService _combatService;
    private readonly ApiConfigService _apiConfigService;
    private readonly FrontierAuthService _frontierAuthService;
    private readonly JournalUploadService _journalUploadService;
    private readonly CompanionUploadService _companionUploadService;
    private readonly VoCoreDisplayService _voCoreDisplayService;
    // Lambdas for wiring ExoBio -> VoCore so we can unsubscribe later
    private EventHandler<ExoBioDiscoveryEventArgs>? _exoDiscoveryAddedHandler;
    private EventHandler<ExoBioDiscoveryEventArgs>? _exoDiscoveryUpdatedHandler;
    private EventHandler<ExoBioDataLoadedEventArgs>? _exoDataLoadedHandler;
    private EventHandler<ExoBioSubmittedEventArgs>? _exoDiscoveriesSubmittedHandler;
    private EventHandler<SystemUIUpdateEventArgs>? _systemUiUpdateHandler;
    private EventHandler<BodyUIUpdateEventArgs>? _bodyUiUpdateHandler;
    private EventHandler<SystemsDataLoadedEventArgs>? _systemsDataLoadedHandler;

    /// <summary>
    /// Event raised when initialization progress updates
    /// </summary>
    public event EventHandler<InitializationProgressEventArgs>? InitializationProgress;

    public StartupService(
        JournalEventService journalEventService,
        ExoBioService exoBioService,
        VisitedSystemsService visitedSystemsService,
        FSDService fsdService,
        RankService rankService,
        OverlayLogService overlayLogService,
        CommanderStatsService commanderStatsService,
        ReputationService reputationService,
        CodexService codexService,
        CombatService combatService,
        ApiConfigService apiConfigService,
        FrontierAuthService frontierAuthService,
        JournalUploadService journalUploadService,
        CompanionUploadService companionUploadService,
        VoCoreDisplayService voCoreDisplayService)
    {
        _journalEventService = journalEventService ?? throw new ArgumentNullException(nameof(journalEventService));
        _exoBioService = exoBioService ?? throw new ArgumentNullException(nameof(exoBioService));
        _visitedSystemsService = visitedSystemsService ?? throw new ArgumentNullException(nameof(visitedSystemsService));
        _fsdService = fsdService ?? throw new ArgumentNullException(nameof(fsdService));
        _rankService = rankService ?? throw new ArgumentNullException(nameof(rankService));
        _overlayLogService = overlayLogService ?? throw new ArgumentNullException(nameof(overlayLogService));
        _commanderStatsService = commanderStatsService ?? throw new ArgumentNullException(nameof(commanderStatsService));
        _reputationService = reputationService ?? throw new ArgumentNullException(nameof(reputationService));
        _codexService = codexService ?? throw new ArgumentNullException(nameof(codexService));
        _combatService = combatService ?? throw new ArgumentNullException(nameof(combatService));
        _apiConfigService = apiConfigService ?? throw new ArgumentNullException(nameof(apiConfigService));
        _frontierAuthService = frontierAuthService ?? throw new ArgumentNullException(nameof(frontierAuthService));
        _journalUploadService = journalUploadService ?? throw new ArgumentNullException(nameof(journalUploadService));
        _companionUploadService = companionUploadService ?? throw new ArgumentNullException(nameof(companionUploadService));
        _voCoreDisplayService = voCoreDisplayService ?? throw new ArgumentNullException(nameof(voCoreDisplayService));
    }

    /// <summary>
    /// Attempts to silently restore a saved Frontier auth session.
    /// </summary>
    public Task TryRestoreAuthAsync() => _frontierAuthService.TryRestoreSessionAsync();

    /// <summary>
    /// Check if this is first run (no save data exists) and scan all journal files if needed
    /// </summary>
    public async Task<bool> InitializeDataAsync()
    {
        try
        {
            // Check that all journal-derived save files exist; any missing file triggers a full re-scan
            // so that newly introduced data types can catch up from history.
            // NOTE: when adding a new service with its own save file, add the filename here.
            var journalDataFiles = new[]
            {
                "exobio_data.json",
                "general_control_data.json",
                "visited_systems_data.json",
                "ranks_data.json",
                "overlay_log_data.json",
                "commander_stats_data.json",
                "reputation_data.json",
                "codex_data.json",
                "combat_data.json",
            };

            bool allFilesPresent = true;
            foreach (var file in journalDataFiles)
            {
                if (!await CheckFileExistsAsync(file))
                {
                    System.Diagnostics.Debug.WriteLine($"Save file missing: {file}");
                    allFilesPresent = false;
                    break;
                }
            }

            // Even when all files are present, check the schema version. If the code requires a
            // newer schema (because a field was added to an existing model), delete all data files
            // so the rescan below rebuilds them with the new fields populated from journal history.
            // NOTE: when adding a field to an existing model that needs journal history, bump
            // RequiredDataSchemaVersion and document the change in the comment on that constant.
            if (allFilesPresent)
            {
                int savedVersion = await LoadSchemaVersionAsync();
                if (savedVersion < RequiredDataSchemaVersion)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Data schema version {savedVersion} < required {RequiredDataSchemaVersion} — deleting data files and performing full rescan");
                    foreach (var file in journalDataFiles)
                        DeleteDataFile(file);
                    allFilesPresent = false;
                }
            }

            // If any data file is absent (first run, new service, or schema bump), scan all journal files from scratch
            if (!allFilesPresent)
            {
                System.Diagnostics.Debug.WriteLine("Performing full scan of all journal files...");
                await ScanAllJournalFilesAsync();
                await SaveSchemaVersionAsync(RequiredDataSchemaVersion);
                // No overlay log to load on first run — entries accumulate from this session onward
                return true; // Indicates initial scan was performed
            }

            System.Diagnostics.Debug.WriteLine("Save data found - loading persisted data...");

            // Load all service data from disk
            await _fsdService.LoadDataAsync();
            await _rankService.LoadDataAsync();
            await _exoBioService.LoadDataAsync();
            await _visitedSystemsService.LoadDataAsync();
            await _overlayLogService.LoadAsync();
            await _commanderStatsService.LoadDataAsync();
            await _reputationService.LoadDataAsync();
            await _codexService.LoadDataAsync();
            await _combatService.LoadDataAsync();

            return false; // No initial scan needed
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during data initialization: {ex.Message}");
            return false;
        }
    }

    private Task<bool> CheckFileExistsAsync(string fileName) =>
        Task.FromResult(File.Exists(GetDataFilePath(fileName)));

    private static string GetDataFilePath(string fileName) =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "SlevinthHeavenEliteDangerous",
            fileName);

    private static void DeleteDataFile(string fileName)
    {
        var path = GetDataFilePath(fileName);
        if (File.Exists(path))
            File.Delete(path);
    }

    private async Task<int> LoadSchemaVersionAsync()
    {
        var path = GetDataFilePath(SchemaVersionFileName);
        if (!File.Exists(path)) return 0;
        try
        {
            var json = await File.ReadAllTextAsync(path);
            var record = JsonSerializer.Deserialize<SchemaVersionRecord>(json);
            return record?.Version ?? 0;
        }
        catch
        {
            return 0;
        }
    }

    private static async Task SaveSchemaVersionAsync(int version)
    {
        var path = GetDataFilePath(SchemaVersionFileName);
        var json = JsonSerializer.Serialize(
            new SchemaVersionRecord { Version = version },
            new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json);
    }

    private sealed class SchemaVersionRecord
    {
        public int Version { get; set; }
    }

    private async Task ScanAllJournalFilesAsync()
    {
        _visitedSystemsService.BeginBulkLoad();
        _overlayLogService.BeginBulkLoad();

        await Task.Run(() =>
        {
            try
            {
                var journalPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    @"Saved Games\Frontier Developments\Elite Dangerous");

                if (!Directory.Exists(journalPath))
                {
                    System.Diagnostics.Debug.WriteLine($"Journal folder not found: {journalPath}");
                    return;
                }

                var logFiles = Directory.GetFiles(journalPath, "*.log")
                    .OrderBy(f => new FileInfo(f).Name)
                    .ToList();

                int processedFiles = 0;
                int totalEvents = 0;

                InitializationProgress?.Invoke(this, new InitializationProgressEventArgs(
                    "Scanning journal files...", 0, logFiles.Count, 0));

                foreach (var filePath in logFiles)
                {
                    var eventsInFile = ProcessJournalFile(filePath);
                    totalEvents += eventsInFile;
                    processedFiles++;

                    InitializationProgress?.Invoke(this, new InitializationProgressEventArgs(
                        "Scanning journal files...", processedFiles, logFiles.Count, totalEvents));
                }

                System.Diagnostics.Debug.WriteLine($"Initial scan complete: {processedFiles} files, {totalEvents} events");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error scanning journal files: {ex.Message}");
            }
        });

        _visitedSystemsService.EndBulkLoad();
        _overlayLogService.EndBulkLoad();
    }

    private int ProcessJournalFile(string filePath)
    {
        int eventCount = 0;

        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Process through journal event service
                if (_journalEventService.ProcessLine(line, filePath))
                {
                    eventCount++;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing file {Path.GetFileName(filePath)}: {ex.Message}");
        }

        return eventCount;
    }

    /// <summary>
    /// Register all event handlers with the journal event service
    /// </summary>
    public void RegisterEventHandlers()
    {
        _journalEventService.RegisterEventHandler(_exoBioService);
        _journalEventService.RegisterEventHandler(_visitedSystemsService);
        _journalEventService.RegisterEventHandler(_fsdService);
        _journalEventService.RegisterEventHandler(_rankService);
        _journalEventService.RegisterEventHandler(_overlayLogService);
        _journalEventService.RegisterEventHandler(_commanderStatsService);
        _journalEventService.RegisterEventHandler(_reputationService);
        _journalEventService.RegisterEventHandler(_codexService);
        _journalEventService.RegisterEventHandler(_combatService);

        // VoCoreDisplayService should listen to ExoBioService events instead of raw journal events
        try
        {
            // Wire ExoBio events to VoCore via small mapping lambdas so VoCore project doesn't need a compile-time
            // dependency on the ExoBio event types.
            _exoDiscoveryAddedHandler = (s, e) =>
            {
                try
                {
                    var d = e.Discovery;
                    _voCoreDisplayService.HandleDiscoveryAdded(
                        d.Key ?? string.Empty,
                        d.Title ?? string.Empty,
                        d.Details ?? string.Empty,
                        d.ScanType ?? string.Empty,
                        d.SampleValue,
                        d.EstimatedValue,
                        d.EstimatedBonus,
                        d.SystemName ?? string.Empty,
                        string.Empty,
                        d.DistanceFromSol);
                }
                catch { }
            };

            _exoDiscoveryUpdatedHandler = (s, e) =>
            {
                try
                {
                    var d = e.Discovery;
                    _voCoreDisplayService.HandleDiscoveryUpdated(d.Key ?? string.Empty, d.ScanType ?? string.Empty, d.EstimatedValue, d.EstimatedBonus);
                }
                catch { }
            };

            _exoDataLoadedHandler = (s, e) =>
            {
                try
                {
                    var list = e.State.Discoveries.Select(d => (
                        d.Key ?? string.Empty,
                        d.Title ?? string.Empty,
                        d.Details ?? string.Empty,
                        d.ScanType ?? string.Empty,
                        d.SampleValue,
                        d.EstimatedValue,
                        d.EstimatedBonus,
                        d.SystemName ?? string.Empty,
                        string.Empty,
                        d.DistanceFromSol));
                    _voCoreDisplayService.HandleDataLoaded(list);
                }
                catch { }
            };

            _exoDiscoveriesSubmittedHandler = (s, e) =>
            {
                try { _voCoreDisplayService.HandleDiscoveriesSubmitted(); } catch { }
            };

            _exoBioService.DiscoveryAdded += _exoDiscoveryAddedHandler;
            _exoBioService.DiscoveryUpdated += _exoDiscoveryUpdatedHandler;
            _exoBioService.DataLoaded += _exoDataLoadedHandler;
            _exoBioService.DiscoveriesSubmitted += _exoDiscoveriesSubmittedHandler;

            // Visited systems -> VoCore wiring: update current system and valuable bodies
            _systemUiUpdateHandler = (s, e) =>
            {
                try
                {
                    var sys = e.System;
                    _voCoreDisplayService.HandleSystemUpdate(sys.StarSystem ?? string.Empty, sys.DistanceFromSol);
                    // Clear valuable bodies when system changes
                    _voCoreDisplayService.HandleValuableBodiesCleared();
                }
                catch { }
            };

            _bodyUiUpdateHandler = (s, e) =>
            {
                try
                {
                    // Only consider bodies for the current system
                    if (e.System == null || e.Body == null) return;

                    var reasons = new List<string>();                    
                    if (SlevinthHeavenEliteDangerous.Services.BodyValueHelper.IsEarthLikeWorld(e.Body.PlanetClass)) reasons.Add("Earth-like");
                    if (SlevinthHeavenEliteDangerous.Services.BodyValueHelper.IsWaterWorld(e.Body.PlanetClass)) reasons.Add("Water world");
                    if (SlevinthHeavenEliteDangerous.Services.BodyValueHelper.HasTerraformState(e.Body.TerraformState)) reasons.Add($"Terraformable ({e.Body.TerraformState})");
                    int bioCount = SlevinthHeavenEliteDangerous.Services.BodyValueHelper.GetBiologicalSignalCount(e.Body.Signals ?? new System.Collections.Generic.List<SlevinthHeavenEliteDangerous.Services.Models.SignalCard>());
                    if (e.Body.Landable && bioCount > 0) reasons.Add($"Landable + Bio ({bioCount})");

                    if (reasons.Count > 0)
                    {
                        var reasonText = string.Join(", ", reasons);
                        _voCoreDisplayService.HandleValuableBodyAdded(e.Body.BodyName ?? $"Body {e.Body.BodyID}", reasonText, e.Body.DistanceFromArrivalLS);
                    }
                }
                catch { }
            };

            _systemsDataLoadedHandler = (s, e) =>
            {
                try
                {
                    var mostRecent = e.Systems.OrderByDescending(sy => sy.LastVisitTimestamp).FirstOrDefault();
                    if (mostRecent != null)
                    {
                        _voCoreDisplayService.HandleSystemUpdate(mostRecent.StarSystem ?? string.Empty, mostRecent.DistanceFromSol);
                        // Build snapshot of valuable bodies for that system (include reasons)
                        var bodies = new List<(string Name, string Reason, double Distance)>();
                        foreach (var b in mostRecent.Bodies ?? new List<BodyCard>())
                        {
                            var reasons = new List<string>();
                            if (SlevinthHeavenEliteDangerous.Services.BodyValueHelper.IsEarthLikeWorld(b.PlanetClass)) reasons.Add("Earth-like");
                            if (SlevinthHeavenEliteDangerous.Services.BodyValueHelper.IsWaterWorld(b.PlanetClass)) reasons.Add("Water world");
                            if (SlevinthHeavenEliteDangerous.Services.BodyValueHelper.HasTerraformState(b.TerraformState)) reasons.Add($"Terraformable ({b.TerraformState})");
                            int bioCount = SlevinthHeavenEliteDangerous.Services.BodyValueHelper.GetBiologicalSignalCount(b.Signals ?? new List<SlevinthHeavenEliteDangerous.Services.Models.SignalCard>());
                            if (b.Landable && bioCount > 0) reasons.Add($"Landable + Bio ({bioCount})");

                        if (reasons.Count > 0)
                        {
                            bodies.Add((b.BodyName ?? $"Body {b.BodyID}", string.Join(", ", reasons), b.DistanceFromArrivalLS));
                        }
                        }
                        _voCoreDisplayService.HandleValuableBodiesSnapshot(bodies);
                    }
                }
                catch { }
            };

            _visitedSystemsService.SystemUIUpdateRequested += _systemUiUpdateHandler;
            _visitedSystemsService.BodyUIUpdateRequested += _bodyUiUpdateHandler;
            _visitedSystemsService.DataLoaded += _systemsDataLoadedHandler;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to hook VoCore to ExoBioService events: {ex.Message}");
        }

        System.Diagnostics.Debug.WriteLine("All event handlers registered with JournalEventService and data services");
    }

    /// <summary>
    /// Run diagnostics on journal files
    /// </summary>
    public void RunDiagnostics()
    {
        // Fire and forget - don't block startup
        _ = _journalEventService.RunDiagnosticsAsync();
    }

    /// <summary>
    /// Start the journal event service monitoring
    /// </summary>
    public void StartJournalMonitoring()
    {
        try
        {
            _journalEventService.Start();
            System.Diagnostics.Debug.WriteLine("Journal monitoring started successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to start journal monitoring: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Start the background journal upload service
    /// </summary>
    public async Task StartJournalUploadAsync()
    {
        try
        {
            await _journalUploadService.StartAsync();
            _companionUploadService.Start();
            System.Diagnostics.Debug.WriteLine("Journal and companion upload services started successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to start journal upload service: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop the journal event service monitoring
    /// </summary>
    public void StopJournalMonitoring()
    {
        try
        {
            // Unhook data-service event subscriptions for VoCore (if we wired them earlier)
            try
            {
                if (_exoDiscoveryAddedHandler != null) _exoBioService.DiscoveryAdded -= _exoDiscoveryAddedHandler;
                if (_exoDiscoveryUpdatedHandler != null) _exoBioService.DiscoveryUpdated -= _exoDiscoveryUpdatedHandler;
                if (_exoDataLoadedHandler != null) _exoBioService.DataLoaded -= _exoDataLoadedHandler;
                if (_exoDiscoveriesSubmittedHandler != null) _exoBioService.DiscoveriesSubmitted -= _exoDiscoveriesSubmittedHandler;
            }
            catch { }

            _journalEventService.Stop();
            _journalEventService.Dispose();
            System.Diagnostics.Debug.WriteLine("Journal monitoring stopped successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping journal monitoring: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop the background journal and companion upload services
    /// </summary>
    public void StopJournalUpload()
    {
        try
        {
            _journalUploadService.Stop();
            _companionUploadService.Stop();
            System.Diagnostics.Debug.WriteLine("Journal and companion upload services stopped successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error stopping journal upload service: {ex.Message}");
        }
    }
}

/// <summary>
/// Event args for initialization progress updates
/// </summary>
public class InitializationProgressEventArgs : EventArgs
{
    public string Message { get; }
    public int ProcessedFiles { get; }
    public int TotalFiles { get; }
    public int TotalEvents { get; }
    public int PercentComplete => TotalFiles > 0 ? (ProcessedFiles * 100 / TotalFiles) : 0;

    public InitializationProgressEventArgs(string message, int processedFiles, int totalFiles, int totalEvents)
    {
        Message = message;
        ProcessedFiles = processedFiles;
        TotalFiles = totalFiles;
        TotalEvents = totalEvents;
    }
}

