using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SlevinthHeavenEliteDangerous.Eddn;

/// <summary>
/// Hosted service that processes EDDN line events, manages per-commander pending state,
/// and sends messages to EDDN via a background channel.
/// </summary>
public sealed class EddnPublisherService : IHostedService, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false,
    };

    private readonly EddnSender _sender;
    private readonly EddnSystemLookupService _lookup;
    private readonly IOptions<EddnOptions> _options;
    private readonly ILogger<EddnPublisherService> _logger;

    private readonly Channel<EddnQueueItem> _channel = Channel.CreateUnbounded<EddnQueueItem>(
        new UnboundedChannelOptions { SingleReader = true });

    private readonly Dictionary<string, EddnCommanderState> _stateCache = new();
    private readonly Dictionary<string, CancellationTokenSource> _saveTimers = new();

    /// <summary>Per-FID locks so the background system-lookup task can't race with ProcessLineAsync.</summary>
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _stateLocks = new();

    private readonly CancellationTokenSource _cts = new();
    private Task _sendTask = Task.CompletedTask;

    public EddnPublisherService(
        EddnSender sender,
        EddnSystemLookupService lookup,
        IOptions<EddnOptions> options,
        ILogger<EddnPublisherService> logger)
    {
        _sender = sender;
        _lookup = lookup;
        _options = options;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Value.Enabled)
        {
            await _lookup.LoadCacheAsync();
            _sendTask = Task.Run(() => SendLoopAsync(_cts.Token));
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _channel.Writer.TryComplete();

        // Flush any debounced saves that haven't fired yet
        var pendingFids = _saveTimers.Keys.ToList();
        foreach (var fid in pendingFids)
        {
            if (_saveTimers.TryGetValue(fid, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
                _saveTimers.Remove(fid);
            }

            if (_stateCache.TryGetValue(fid, out var state))
                await SaveStateAsync(fid, state);
        }

        try { await _sendTask; }
        catch (OperationCanceledException) { }
    }

    public void Dispose()
    {
        _cts.Dispose();
        foreach (var cts in _saveTimers.Values) cts.Dispose();
        foreach (var sem in _stateLocks.Values) sem.Dispose();
    }

    /// <summary>
    /// Process a single journal line for the given commander.
    /// Called from JournalProcessingService after the main line processor.
    /// </summary>
    public async Task ProcessLineAsync(string jsonLine, string fid, CancellationToken ct)
    {
        if (!_options.Value.Enabled) return;

        var stateLock = GetStateLock(fid);
        await stateLock.WaitAsync(ct);
        try
        {
            var state = await GetOrLoadStateAsync(fid);

            var prevSystemAddress = state.CurrentSystemAddress;
            var prevNavPendingCount = state.PendingNavEvents.Count;

            var result = EddnLineProcessor.ProcessLine(jsonLine, state);

            var newSystemAddress = state.CurrentSystemAddress;
            var navPendingAdded = state.PendingNavEvents.Count > prevNavPendingCount;

            bool mutated = result.StateMutated;

            foreach (var ev in result.EventsToSend)
            {
                if (state.SentEventKeys.Add(ev.EventKey))
                {
                    EnsureStarPosOnPendingEvent(ev, state);
                    _channel.Writer.TryWrite(new EddnQueueItem(BuildMessage(ev, state.GameInfo), 0, DateTimeOffset.UtcNow));
                    mutated = true;
                }
            }

            if (mutated)
                ScheduleSave(fid, state);

            // Trigger a system lookup when:
            //  (a) we entered a new system — kicks off the initial async HTTP check, or
            //  (b) new pending nav events were added for the current system —
            //      if the system is already cached as known, releases them immediately
            //      without waiting for SellExplorationData.
            // This covers Scan, FSSAllBodiesFound, ScanBaryCentre, ApproachSettlement, etc.
            if (_options.Value.SystemLookupEnabled
                && newSystemAddress.HasValue
                && state.CurrentSystem != null
                && (newSystemAddress != prevSystemAddress || navPendingAdded))
            {
                TriggerSystemCheck(newSystemAddress.Value, state.CurrentSystem, fid);
            }
        }
        finally
        {
            stateLock.Release();
        }
    }

    /// <summary>
    /// Process a companion file (Market.json, Shipyard.json, etc.) and send immediately to EDDN.
    /// Companion file data is not discovery-sensitive so it bypasses the pending queue.
    /// </summary>
    public async Task ProcessCompanionAsync(string type, string json, string fid, CancellationToken ct)
    {
        if (!_options.Value.Enabled) return;

        var stateLock = GetStateLock(fid);
        await stateLock.WaitAsync(ct);
        try
        {
            var state = await GetOrLoadStateAsync(fid);
            var ev = EddnCompanionProcessor.Process(type, json, state.GameInfo);
            if (ev == null) return;

            _channel.Writer.TryWrite(new EddnQueueItem(BuildMessage(ev, state.GameInfo), 0, DateTimeOffset.UtcNow));
        }
        finally
        {
            stateLock.Release();
        }
    }

    /// <summary>
    /// Resets EDDN state for a commander — called before a full journal reprocess.
    /// Clears the cache and deletes the state file so pending events are rebuilt from scratch.
    /// </summary>
    public void ResetCommanderState(string fid)
    {
        if (!_options.Value.Enabled) return;

        if (_saveTimers.TryGetValue(fid, out var existingCts))
        {
            existingCts.Cancel();
            existingCts.Dispose();
            _saveTimers.Remove(fid);
        }

        _stateCache.Remove(fid);

        var path = StatePath(fid);
        if (File.Exists(path))
        {
            try { File.Delete(path); }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EDDN] Failed to delete state file for {FID}", fid);
            }
        }
    }

    // ----- system lookup -----

    private void TriggerSystemCheck(long systemAddress, string systemName, string fid)
    {
        if (_lookup.TryGetCached(systemAddress, out bool isKnown))
        {
            if (isKnown)
                _ = ReleasePendingForSystemAsync(systemAddress, fid);
            // "unknown" cached result — stay held
            return;
        }

        // Not in cache — check async without blocking line processing
        _ = CheckAndReleaseSystemAsync(systemAddress, systemName, fid);
    }

    private async Task CheckAndReleaseSystemAsync(long systemAddress, string systemName, string fid)
    {
        try
        {
            var isKnown = await _lookup.CheckSystemAsync(systemAddress, systemName, _cts.Token);
            if (isKnown)
                await ReleasePendingForSystemAsync(systemAddress, fid);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[EDDN] System check failed for {System} — events remain held", systemName);
        }
    }

    private async Task ReleasePendingForSystemAsync(long systemAddress, string fid)
    {
        var stateLock = GetStateLock(fid);
        await stateLock.WaitAsync(_cts.Token);
        try
        {
            if (!_stateCache.TryGetValue(fid, out var state)) return;

            var toRelease = state.PendingNavEvents
                .Where(e => e.SystemAddress == systemAddress)
                .ToList();

            if (toRelease.Count == 0) return;

            foreach (var ev in toRelease)
            {
                state.PendingNavEvents.Remove(ev);
                if (state.SentEventKeys.Add(ev.EventKey))
                {
                    EnsureStarPosOnPendingEvent(ev, state);
                    _channel.Writer.TryWrite(new EddnQueueItem(BuildMessage(ev, state.GameInfo), 0, DateTimeOffset.UtcNow));
                }
            }

            ScheduleSave(fid, state);

            _logger.LogInformation(
                "[EDDN] Released {Count} pending event(s) for known system {Address}",
                toRelease.Count, systemAddress);
        }
        catch (OperationCanceledException) { }
        finally
        {
            stateLock.Release();
        }
    }

    private SemaphoreSlim GetStateLock(string fid) =>
        _stateLocks.GetOrAdd(fid, _ => new SemaphoreSlim(1, 1));

    // ----- private helpers -----

    private async Task<EddnCommanderState> GetOrLoadStateAsync(string fid)
    {
        if (_stateCache.TryGetValue(fid, out var cached))
            return cached;

        var path = StatePath(fid);
        if (File.Exists(path))
        {
            try
            {
                var json = await File.ReadAllTextAsync(path);
                var state = JsonSerializer.Deserialize<EddnCommanderState>(json, JsonOptions);
                if (state != null)
                {
                    _stateCache[fid] = state;
                    return state;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EDDN] Failed to load state for {FID} — starting fresh", fid);
            }
        }

        var fresh = new EddnCommanderState();
        _stateCache[fid] = fresh;
        return fresh;
    }

    private void ScheduleSave(string fid, EddnCommanderState state)
    {
        if (_saveTimers.TryGetValue(fid, out var existing))
        {
            existing.Cancel();
            existing.Dispose();
        }

        var cts = new CancellationTokenSource();
        _saveTimers[fid] = cts;
        var token = cts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(500, token);
                if (!token.IsCancellationRequested)
                    await SaveStateAsync(fid, state);
            }
            catch (OperationCanceledException) { }
        }, token);
    }

    private async Task SaveStateAsync(string fid, EddnCommanderState state)
    {
        var path = StatePath(fid);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(state, JsonOptions);
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[EDDN] Failed to save state for {FID}", fid);
        }
    }

    private string StatePath(string fid) =>
        Path.Combine(_options.Value.StoragePath, fid, "_eddn_state.json");

    private EddnMessage BuildMessage(EddnPendingEvent ev, EddnGameInfo gameInfo) =>
        new()
        {
            SchemaRef = ev.SchemaRef,
            UploaderID = gameInfo.CommanderName,
            SoftwareName = _options.Value.SoftwareName,
            SoftwareVersion = _options.Value.SoftwareVersion,
            GameVersion = gameInfo.GameVersion,
            GameBuild = gameInfo.GameBuild,
            MessageJson = ev.MessageJson,
        };

    private void EnsureStarPosOnPendingEvent(EddnPendingEvent ev, EddnCommanderState state)
    {
        try
        {
            var node = System.Text.Json.Nodes.JsonNode.Parse(ev.MessageJson) as System.Text.Json.Nodes.JsonObject;
            if (node == null) return;

            if (node.ContainsKey("StarPos")) return;

            if (state.SystemPositions != null && state.SystemPositions.TryGetValue(ev.SystemAddress, out var pos) && pos != null)
            {
                var arr = new System.Text.Json.Nodes.JsonArray();
                foreach (var d in pos) arr.Add(d);
                node["StarPos"] = arr;
                ev.MessageJson = node.ToJsonString();
            }
        }
        catch { }
    }

    private async Task SendLoopAsync(CancellationToken ct)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
        {
            // Wait until the scheduled send time (used for retries)
            var delay = item.SendAfter - DateTimeOffset.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                try { await Task.Delay(delay, ct); }
                catch (OperationCanceledException) { return; }
            }

            EddnSendResult result;
            try { result = await _sender.SendAsync(item.Message, ct); }
            catch (OperationCanceledException) { return; }

            if (result.Success) continue;

            // 400 Bad Request / 426 Upgrade Required — discard, no retry
            if (result.StatusCode is 400 or 426)
            {
                _logger.LogWarning("[EDDN] Discarding message (status {Status}): {Error}",
                    result.StatusCode, result.Error);
                continue;
            }

            // Other failure — retry up to 3 times with 1-minute backoff
            if (item.RetryCount < 3)
            {
                var retry = item with
                {
                    RetryCount = item.RetryCount + 1,
                    SendAfter = DateTimeOffset.UtcNow.AddMinutes(1),
                };
                _channel.Writer.TryWrite(retry);
                _logger.LogWarning("[EDDN] Queued retry {Attempt}/3 for failed message", item.RetryCount + 1);
            }
            else
            {
                _logger.LogError("[EDDN] Message failed after 3 retries — discarding");
            }
        }
    }
}

internal sealed record EddnQueueItem(EddnMessage Message, int RetryCount, DateTimeOffset SendAfter);
