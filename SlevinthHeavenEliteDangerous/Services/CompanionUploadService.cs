using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Watches Elite Dangerous companion JSON files (Market.json, Shipyard.json, etc.)
/// and uploads them to the API for EDDN forwarding whenever the game writes them.
/// </summary>
public sealed class CompanionUploadService : IDisposable
{
    private static readonly string JournalPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        @"Saved Games\Frontier Developments\Elite Dangerous");

    /// <summary>Companion file names and their corresponding API type identifiers.</summary>
    private static readonly Dictionary<string, string> CompanionFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Market.json",      "market"      },
        { "Shipyard.json",    "shipyard"    },
        { "Outfitting.json",  "outfitting"  },
        { "FCMaterials.json", "fcmaterials" },
    };

    private readonly ISlevinthHeavenApi _api;
    private readonly JournalUploadService _journalUpload;
    private readonly FrontierAuthService _authService;

    private FileSystemWatcher? _watcher;

    // Debounce: track last scheduled upload per file name so rapid writes don't spam the API.
    private readonly Dictionary<string, CancellationTokenSource> _debounceTokens = new();

    // Dedup: only upload when file content has actually changed.
    private readonly Dictionary<string, string> _lastUploadedContent = new(StringComparer.OrdinalIgnoreCase);

    public CompanionUploadService(
        ISlevinthHeavenApi api,
        JournalUploadService journalUpload,
        FrontierAuthService authService)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _journalUpload = journalUpload ?? throw new ArgumentNullException(nameof(journalUpload));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    public void Start()
    {
        if (!Directory.Exists(JournalPath))
        {
            Debug.WriteLine("[CompanionUpload] Journal folder not found — companion watching disabled.");
            return;
        }

        _watcher = new FileSystemWatcher(JournalPath)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
            EnableRaisingEvents = true,
        };

        foreach (var fileName in CompanionFiles.Keys)
            _watcher.Filters.Add(fileName);

        _watcher.Changed += OnFileChanged;

        Debug.WriteLine("[CompanionUpload] Watching for companion file changes.");
    }

    public void Stop()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Changed -= OnFileChanged;
        }

        foreach (var cts in _debounceTokens.Values)
            cts.Cancel();
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        var fileName = Path.GetFileName(e.FullPath);
        if (!CompanionFiles.TryGetValue(fileName, out var type)) return;
        if (!_authService.IsAuthenticated) return;

        // Debounce: cancel any pending upload for this file, schedule a new one
        if (_debounceTokens.TryGetValue(fileName, out var existing))
        {
            existing.Cancel();
            existing.Dispose();
        }

        var cts = new CancellationTokenSource();
        _debounceTokens[fileName] = cts;

        _ = UploadAfterDelayAsync(e.FullPath, fileName, type, cts.Token);
    }

    private async Task UploadAfterDelayAsync(
        string filePath, string fileName, string type, CancellationToken ct)
    {
        try
        {
            // Wait for the game to finish writing
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
            if (ct.IsCancellationRequested) return;

            var fid = _journalUpload.FID;
            if (string.IsNullOrEmpty(fid))
            {
                Debug.WriteLine("[CompanionUpload] FID not yet known — skipping.");
                return;
            }

            string json;
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                json = await reader.ReadToEndAsync(ct);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CompanionUpload] Failed to read {fileName}: {ex.Message}");
                return;
            }

            if (string.IsNullOrWhiteSpace(json)) return;

            // Skip if content hasn't changed since last upload
            if (_lastUploadedContent.TryGetValue(fileName, out var last) && last == json) return;

            await UploadAsync(fileName, type, json, fid, ct);
            _lastUploadedContent[fileName] = json;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Debug.WriteLine($"[CompanionUpload] Error uploading {fileName}: {ex.Message}");
        }
    }

    private async Task UploadAsync(string fileName, string type, string json, string fid, CancellationToken ct)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(type), "type");
            content.Add(new StringContent(fid), "fid");

            var fileContent = new StringContent(json, Encoding.UTF8, "application/json");
            content.Add(fileContent, "file", fileName);

            await _api.UploadCompanionFileAsync(content, ct);

            Debug.WriteLine($"[CompanionUpload] Uploaded {fileName} ({type})");
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"[CompanionUpload] Network error uploading {fileName}: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Stop();
        _watcher?.Dispose();
        foreach (var cts in _debounceTokens.Values) cts.Dispose();
    }
}
