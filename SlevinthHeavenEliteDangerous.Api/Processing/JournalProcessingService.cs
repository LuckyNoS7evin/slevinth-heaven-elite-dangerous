using SlevinthHeavenEliteDangerous.Api.Models;
using SlevinthHeavenEliteDangerous.Api.Storage;
using SlevinthHeavenEliteDangerous.Eddn;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Api.Processing;

/// <summary>
/// Background service that periodically processes raw journal files
/// saved by <see cref="JournalFileStore"/> into <see cref="ServerCommanderData"/>
/// via <see cref="CommanderDataStore"/>.
/// Tracks progress per-file so only new or changed files are processed.
/// </summary>
public sealed class JournalProcessingService(
    JournalFileStore journalStore,
    CommanderDataStore commanderStore,
    EddnPublisherService eddnPublisher,
    ILogger<JournalProcessingService> logger) : BackgroundService
{
    private static readonly TimeSpan ProcessingInterval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Increment this when new event types are added to <see cref="JournalLineProcessor"/>.
    /// Any commander whose manifest has a lower version will be fully reprocessed on the next cycle.
    ///
    /// History:
    ///   1 — initial versioned baseline
    ///   2 — EDDN integration: forces full reprocess so EddnPublisherService rebuilds pending state
    /// </summary>
    private const int CurrentProcessingSchemaVersion = 2;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("[JournalProcessing] Service starting — waiting 5s before first cycle");

        // Give the app time to start up
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAllCommandersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "[JournalProcessing] Unhandled error during processing cycle");
            }

            await Task.Delay(ProcessingInterval, stoppingToken);
        }
    }

    private async Task ProcessAllCommandersAsync(CancellationToken ct)
    {
        var fids = journalStore.ListFIDs().ToList();
        logger.LogInformation("[JournalProcessing] Processing cycle — found {Count} commander(s)", fids.Count);

        foreach (var fid in fids)
        {
            if (ct.IsCancellationRequested) break;

            try
            {
                await ProcessCommanderAsync(fid, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "[JournalProcessing] Error processing commander {FID}", fid);
            }
        }
    }

    private async Task ProcessCommanderAsync(string fid, CancellationToken ct)
    {
        var manifest = await LoadManifestAsync(fid);
        var journalFiles = journalStore.GetJournalFiles(fid); // sorted by name (chronological)

        logger.LogInformation(
            "[JournalProcessing] Commander {FID}: {TotalFiles} journal file(s), {ManifestCount} in manifest",
            fid, journalFiles.Count, manifest.Files.Count);

        // If the processing schema version has been bumped (new event types added to JournalLineProcessor),
        // reset the manifest so every file is reprocessed from scratch on this cycle.
        if (manifest.SchemaVersion < CurrentProcessingSchemaVersion)
        {
            logger.LogInformation(
                "[JournalProcessing] Processing schema version {Saved} < required {Required} for {FID} — forcing full reprocess",
                manifest.SchemaVersion, CurrentProcessingSchemaVersion, fid);
            manifest = new ProcessingManifest();
        }

        // Find which files need processing (new or changed size)
        // For files that grew, record how many lines were already processed so we can skip them.
        var pendingFiles = new List<(string Path, int SkipLines)>();
        foreach (var filePath in journalFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var fileSize = new FileInfo(filePath).Length;

            if (!manifest.Files.TryGetValue(fileName, out var entry))
            {
                pendingFiles.Add((filePath, 0)); // brand-new file
            }
            else if (entry.FileSize != fileSize)
            {
                pendingFiles.Add((filePath, entry.LinesProcessed)); // file grew — skip already-processed lines
            }
        }

        if (pendingFiles.Count == 0)
            return;

        logger.LogInformation(
            "[JournalProcessing] Commander {FID}: {PendingCount} file(s) pending processing",
            fid, pendingFiles.Count);

        // Determine whether a full reprocess from scratch is needed.
        // Case 1: manifest is empty (files deleted / first run / schema bump) — cached data may have stale watermarks.
        // Case 2: pending files are chronologically BEFORE the latest already-processed file
        //         (out-of-order arrival) — events in earlier files would be missed.
        bool fullReprocess = false;
        if (manifest.Files.Count == 0)
        {
            fullReprocess = true;
        }
        else
        {
            var lastProcessedName = manifest.Files.Keys.Max(StringComparer.Ordinal);
            var earliestPendingName = Path.GetFileName(pendingFiles[0].Path); // already sorted

            if (string.Compare(earliestPendingName, lastProcessedName, StringComparison.Ordinal) < 0)
                fullReprocess = true;
        }

        ServerCommanderData data;
        List<(string Path, int SkipLines)> filesToProcess;

        if (fullReprocess)
        {
            logger.LogInformation(
                "[JournalProcessing] Full reprocess for {FID} — rebuilding from all {Count} files",
                fid, journalFiles.Count);

            // Fresh data + clear manifest so every file is reprocessed in order (skip nothing).
            // Preserve metadata fields that are not derived from journal events.
            var existing = await commanderStore.GetOrCreateAsync(fid);
            manifest = new ProcessingManifest();
            data = new ServerCommanderData { FID = fid, LastAppVersion = existing.LastAppVersion };
            filesToProcess = journalFiles.Select(f => (f, 0)).ToList();
            eddnPublisher.ResetCommanderState(fid);
        }
        else
        {
            data = await commanderStore.GetOrCreateAsync(fid);
            filesToProcess = pendingFiles;
        }

        foreach (var (filePath, skipLines) in filesToProcess)
        {
            if (ct.IsCancellationRequested) break;

            var fileName = Path.GetFileName(filePath);
            var fileSize = new FileInfo(filePath).Length;

            int newLines = await ProcessFileAsync(filePath, fid, data, skipLines, fullReprocess, ct);
            int totalLines = skipLines + newLines;

            manifest.Files[fileName] = new ProcessedFileEntry
            {
                LastProcessedUtc = DateTime.UtcNow,
                FileSize = fileSize,
                LinesProcessed = totalLines,
            };

            logger.LogInformation(
                "[JournalProcessing] Processed {File} for {FID}: {NewLines} new lines ({TotalLines} total, skipped {Skipped})",
                fileName, fid, newLines, totalLines, skipLines);
        }

        data.LastUploadTimestamp = DateTime.UtcNow;
        manifest.SchemaVersion = CurrentProcessingSchemaVersion;
        await commanderStore.SaveAsync(data);
        await SaveManifestAsync(fid, manifest);
    }

    private async Task<int> ProcessFileAsync(
        string filePath, string fid, ServerCommanderData data, int skipLines, bool isReprocess, CancellationToken ct)
    {
        int lineNumber = 0;
        int processed = 0;

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            lineNumber++;

            // Skip lines already processed in a previous cycle
            if (lineNumber <= skipLines)
                continue;

            try
            {
                JournalLineProcessor.ProcessLine(line, data);

                // Don't send historical data to EDDN during a full reprocess —
                // only forward events that are genuinely new (incremental processing).
                if (!isReprocess)
                    await eddnPublisher.ProcessLineAsync(line, fid, ct);

                processed++;
            }
            catch
            {
                // skip bad lines
            }
        }

        return processed;
    }

    // ---- Manifest persistence ----

    private async Task<ProcessingManifest> LoadManifestAsync(string fid)
    {
        var path = ManifestPath(fid);
        if (!File.Exists(path))
            return new ProcessingManifest();

        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<ProcessingManifest>(json, JsonOptions)
                ?? new ProcessingManifest();
        }
        catch
        {
            return new ProcessingManifest();
        }
    }

    private async Task SaveManifestAsync(string fid, ProcessingManifest manifest)
    {
        var path = ManifestPath(fid);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var json = JsonSerializer.Serialize(manifest, JsonOptions);
        await File.WriteAllTextAsync(path, json);
    }

    private string ManifestPath(string fid) =>
        Path.Combine(journalStore.GetCommanderDirectory(fid), "_processed.json");
}

/// <summary>
/// Tracks which journal files have been processed for a given commander.
/// </summary>
public class ProcessingManifest
{
    /// <summary>
    /// The <see cref="JournalProcessingService.CurrentProcessingSchemaVersion"/> at the time this
    /// manifest was last written. A mismatch triggers a full reprocess.
    /// </summary>
    public int SchemaVersion { get; set; }

    public Dictionary<string, ProcessedFileEntry> Files { get; set; } = new();
}

public class ProcessedFileEntry
{
    public DateTime LastProcessedUtc { get; set; }
    public long FileSize { get; set; }
    public int LinesProcessed { get; set; }
}
