using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SlevinthHeavenEliteDangerous.Eddn;

/// <summary>
/// Checks whether a star system is already known to the community database (EDSM by default).
/// Results are cached in memory and persisted to disk so known systems are never re-checked.
/// Unknown results expire after 24 hours and are re-checked on the next encounter.
/// </summary>
public sealed class EddnSystemLookupService : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };
    private static readonly TimeSpan UnknownExpiry = TimeSpan.FromHours(24);
    private static readonly TimeSpan MinRequestInterval = TimeSpan.FromSeconds(1);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<EddnOptions> _options;
    private readonly ILogger<EddnSystemLookupService> _logger;

    /// <summary>In-memory cache: systemAddress → (isKnown, checkedAt).</summary>
    private readonly ConcurrentDictionary<long, CacheEntry> _cache = new();

    /// <summary>Tracks systems currently being looked up to avoid duplicate in-flight requests.</summary>
    private readonly ConcurrentDictionary<long, bool> _inFlight = new();

    private readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private DateTime _lastRequestUtc = DateTime.MinValue;

    private CancellationTokenSource? _saveCts;

    public EddnSystemLookupService(
        IHttpClientFactory httpClientFactory,
        IOptions<EddnOptions> options,
        ILogger<EddnSystemLookupService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Load the persisted cache from disk. Call once at startup.
    /// </summary>
    public async Task LoadCacheAsync()
    {
        var path = CachePath();
        if (!File.Exists(path)) return;

        try
        {
            var json = await File.ReadAllTextAsync(path);
            var entries = JsonSerializer.Deserialize<Dictionary<string, bool>>(json, JsonOptions);
            if (entries == null) return;

            foreach (var (key, isKnown) in entries)
            {
                if (long.TryParse(key, out var addr))
                    _cache[addr] = new CacheEntry(isKnown, DateTime.UtcNow);
            }

            _logger.LogInformation("[EDDN] Loaded {Count} known systems from cache", _cache.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[EDDN] Failed to load system cache — starting fresh");
        }
    }

    /// <summary>
    /// Check whether a result is already cached.
    /// Returns false if the system has not been checked or the "unknown" result has expired.
    /// </summary>
    public bool TryGetCached(long systemAddress, out bool isKnown)
    {
        if (_cache.TryGetValue(systemAddress, out var entry))
        {
            if (entry.IsKnown || DateTime.UtcNow - entry.CheckedAt < UnknownExpiry)
            {
                isKnown = entry.IsKnown;
                return true;
            }

            // Expired unknown — remove so it gets re-checked
            _cache.TryRemove(systemAddress, out _);
        }

        isKnown = false;
        return false;
    }

    /// <summary>
    /// Look up a system via the configured API.
    /// Rate-limited to one request per second.
    /// Deduplicates concurrent requests for the same system.
    /// </summary>
    public async Task<bool> CheckSystemAsync(long systemAddress, string systemName, CancellationToken ct)
    {
        // Don't fire duplicate in-flight requests.
        // Return false (keep events held) — TriggerSystemCheck will retry naturally
        // when the next relevant event for this system is processed.
        if (!_inFlight.TryAdd(systemAddress, true))
            return false;

        try
        {
            await _rateLimiter.WaitAsync(ct);
            try
            {
                var elapsed = DateTime.UtcNow - _lastRequestUtc;
                if (elapsed < MinRequestInterval)
                    await Task.Delay(MinRequestInterval - elapsed, ct);

                var url = BuildUrl(systemAddress, systemName);
                var client = _httpClientFactory.CreateClient("edsm");
                var response = await client.GetStringAsync(url, ct);
                _lastRequestUtc = DateTime.UtcNow;

                var isKnown = IsKnownResponse(response);
                _cache[systemAddress] = new CacheEntry(isKnown, DateTime.UtcNow);

                if (isKnown)
                    ScheduleCacheSave();

                _logger.LogDebug("[EDDN] System {Name} ({Address}): {Status}",
                    systemName, systemAddress, isKnown ? "known" : "unknown");

                return isKnown;
            }
            finally
            {
                _rateLimiter.Release();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[EDDN] System lookup failed for {Name} ({Address})", systemName, systemAddress);
            return false; // Fail safe: keep events held
        }
        finally
        {
            _inFlight.TryRemove(systemAddress, out _);
        }
    }

    private string BuildUrl(long systemAddress, string systemName)
    {
        var baseUrl = _options.Value.SystemLookupApiUrl.TrimEnd('/');
        // EDSM: /api-v1/system?sysId64=X  — Spansh or others: configurable
        return $"{baseUrl}?sysId64={systemAddress}";
    }

    private static bool IsKnownResponse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return root.ValueKind == JsonValueKind.Object && root.EnumerateObject().Any();
        }
        catch
        {
            return false;
        }
    }

    private void ScheduleCacheSave()
    {
        _saveCts?.Cancel();
        _saveCts?.Dispose();
        var cts = new CancellationTokenSource();
        _saveCts = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(2000, cts.Token);
                if (!cts.Token.IsCancellationRequested)
                    await SaveCacheAsync();
            }
            catch (OperationCanceledException) { }
        }, cts.Token);
    }

    private async Task SaveCacheAsync()
    {
        var path = CachePath();
        try
        {
            // Only persist known entries — unknown entries expire and aren't worth persisting
            var toSave = _cache
                .Where(kvp => kvp.Value.IsKnown)
                .ToDictionary(kvp => kvp.Key.ToString(), kvp => true);

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(toSave, JsonOptions);
            await File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[EDDN] Failed to save system cache");
        }
    }

    private string CachePath() =>
        Path.Combine(_options.Value.StoragePath, "_eddn_system_cache.json");

    public void Dispose()
    {
        _rateLimiter.Dispose();
        _saveCts?.Dispose();
    }

    private sealed record CacheEntry(bool IsKnown, DateTime CheckedAt);
}
