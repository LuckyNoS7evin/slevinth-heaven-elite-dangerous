using SlevinthHeavenEliteDangerous.Api.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SlevinthHeavenEliteDangerous.Api.Storage;

/// <summary>
/// Thread-safe, file-backed storage for <see cref="ServerCommanderData"/>.
/// Each commander is stored as <c>Data/Commanders/{FID}.json</c>.
/// Data is cached in memory and written to disk after every mutation.
/// </summary>
public sealed class CommanderDataStore
{
    private readonly string _basePath;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
    private readonly ConcurrentDictionary<string, ServerCommanderData> _cache = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    public CommanderDataStore(IWebHostEnvironment env)
    {
        _basePath = Path.Combine(env.ContentRootPath, "Data", "Commanders");
        Directory.CreateDirectory(_basePath);
    }

    /// <summary>
    /// Get the commander data for the given Frontier ID.
    /// Returns null if no data exists.
    /// </summary>
    public async Task<ServerCommanderData?> GetAsync(string fid)
    {
        if (string.IsNullOrWhiteSpace(fid))
            return null;

        fid = Sanitise(fid);

        if (_cache.TryGetValue(fid, out var cached))
            return cached;

        var sem = GetLock(fid);
        await sem.WaitAsync();
        try
        {
            // double-check after lock
            if (_cache.TryGetValue(fid, out cached))
                return cached;

            var path = FilePath(fid);
            if (!File.Exists(path))
                return null;

            var json = await File.ReadAllTextAsync(path);
            var data = JsonSerializer.Deserialize<ServerCommanderData>(json, JsonOptions);
            if (data != null)
                _cache[fid] = data;

            return data;
        }
        finally
        {
            sem.Release();
        }
    }

    /// <summary>
    /// Get or create commander data for the given Frontier ID.
    /// </summary>
    public async Task<ServerCommanderData> GetOrCreateAsync(string fid)
    {
        fid = Sanitise(fid);
        var data = await GetAsync(fid);
        if (data != null)
            return data;

        var newData = new ServerCommanderData { FID = fid };
        _cache[fid] = newData;
        return newData;
    }

    /// <summary>
    /// Persist the commander data to disk.
    /// </summary>
    public async Task SaveAsync(ServerCommanderData data)
    {
        var fid = Sanitise(data.FID);
        _cache[fid] = data;

        var sem = GetLock(fid);
        await sem.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            await File.WriteAllTextAsync(FilePath(fid), json);
        }
        finally
        {
            sem.Release();
        }
    }

    /// <summary>
    /// List all commander FIDs that have data stored.
    /// </summary>
    public IEnumerable<string> ListFIDs()
    {
        if (!Directory.Exists(_basePath))
            return [];

        return Directory.GetFiles(_basePath, "*.json")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(f => !string.IsNullOrEmpty(f))!;
    }

    /// <summary>
    /// Return basic profile summaries for all stored commanders.
    /// </summary>
    public async Task<List<ServerCommanderData>> GetAllAsync()
    {
        var result = new List<ServerCommanderData>();
        foreach (var fid in ListFIDs())
        {
            var data = await GetAsync(fid);
            if (data != null)
                result.Add(data);
        }
        return result;
    }

    private string FilePath(string fid) => Path.Combine(_basePath, $"{fid}.json");

    private SemaphoreSlim GetLock(string fid) =>
        _locks.GetOrAdd(fid, _ => new SemaphoreSlim(1, 1));

    private static string Sanitise(string fid) =>
        string.Concat(fid.Where(c => char.IsLetterOrDigit(c) || c == '-'));
}
