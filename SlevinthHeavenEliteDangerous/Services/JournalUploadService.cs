using SlevinthHeavenEliteDangerous.Data;
using SlevinthHeavenEliteDangerous.Services.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.Services;

/// <summary>
/// Periodically uploads raw journal files to the server.
/// Tracks per-file size so only new or changed files are uploaded.
/// Resilient to connectivity failures — retries on the next cycle.
/// </summary>
public sealed class JournalUploadService : IDisposable
{
    private static readonly TimeSpan UploadInterval = TimeSpan.FromMinutes(5);

    private static readonly string JournalPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        @"Saved Games\Frontier Developments\Elite Dangerous");

    private readonly ISlevinthHeavenApi _api;
    private readonly FrontierAuthService _authService;
    private readonly DataService<JournalUploadState> _stateService;
    private JournalUploadState _state;
    private readonly CancellationTokenSource _cts = new();
    private Task? _uploadLoop;

    public JournalUploadService(ISlevinthHeavenApi api, FrontierAuthService authService)
    {
        _api = api ?? throw new ArgumentNullException(nameof(api));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _stateService = new DataService<JournalUploadState>("journal_upload_state.json");
        _state = new JournalUploadState();
    }

    /// <summary>
    /// The commander's FID, once discovered from journal files.
    /// </summary>
    public string? FID => _state.FID;

    /// <summary>
    /// Start the background upload loop.
    /// </summary>
    public async Task StartAsync()
    {
        var ct = _cts.Token;

        _state = await _stateService.LoadDataAsync() ?? new JournalUploadState();

        // Discover FID if not yet known
        if (string.IsNullOrEmpty(_state.FID))
        {
            _state.FID = await Task.Run(() => DiscoverFID(), ct);
            if (ct.IsCancellationRequested) return;
            if (!string.IsNullOrEmpty(_state.FID))
                await _stateService.SaveDataAsync(_state);
        }

        _uploadLoop = UploadLoopAsync(ct);

        await ReportVersionAsync();
    }

    private async Task ReportVersionAsync()
    {
        if (string.IsNullOrEmpty(_state.FID)) return;

        try
        {
            await _api.ReportVersionAsync(new Dictionary<string, string>
            {
                ["fid"] = _state.FID,
            });

            Debug.WriteLine($"[JournalUpload] Reported app version for FID {_state.FID}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[JournalUpload] Failed to report version: {ex.Message}");
        }
    }

    /// <summary>
    /// Stop the background upload loop gracefully.
    /// </summary>
    public void Stop()
    {
        _cts.Cancel();
    }

    private async Task UploadLoopAsync(CancellationToken ct)
    {
        // Small initial delay so the app finishes startup
        await Task.Delay(TimeSpan.FromSeconds(15), ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await UploadPendingFilesAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JournalUpload] Error during upload cycle: {ex.Message}");
            }

            try { await Task.Delay(UploadInterval, ct); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task UploadPendingFilesAsync(CancellationToken ct)
    {
        if (!_authService.IsAuthenticated)
        {
            Debug.WriteLine("[JournalUpload] Not authenticated — skipping upload cycle.");
            return;
        }

        // Re-discover FID if we don't have one yet
        if (string.IsNullOrEmpty(_state.FID))
        {
            _state.FID = DiscoverFID();
            if (string.IsNullOrEmpty(_state.FID))
            {
                Debug.WriteLine("[JournalUpload] FID not found in journal files — skipping.");
                return;
            }
            await _stateService.SaveDataAsync(_state);
        }

        if (!Directory.Exists(JournalPath))
        {
            Debug.WriteLine($"[JournalUpload] Journal folder not found: {JournalPath}");
            return;
        }

        var logFiles = Directory.GetFiles(JournalPath, "*.log")
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        bool stateChanged = false;

        foreach (var filePath in logFiles)
        {
            if (ct.IsCancellationRequested) break;

            var fileName = Path.GetFileName(filePath);
            var fileSize = new FileInfo(filePath).Length;

            // Skip files already uploaded at this exact size
            if (_state.UploadedFiles.TryGetValue(fileName, out var uploadedSize) && uploadedSize == fileSize)
                continue;

            try
            {
                await UploadFileAsync(filePath, fileName, _state.FID, ct);
                _state.UploadedFiles[fileName] = fileSize;
                stateChanged = true;

                Debug.WriteLine($"[JournalUpload] Uploaded {fileName} ({fileSize:N0} bytes)");
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[JournalUpload] Network error uploading {fileName}: {ex.Message} — will retry next cycle.");
                break; // Stop trying this cycle — server may be unreachable
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[JournalUpload] Error uploading {fileName}: {ex.Message}");
            }
        }

        if (stateChanged)
            await _stateService.SaveDataAsync(_state);
    }

    private async Task UploadFileAsync(string filePath, string fileName, string fid, CancellationToken ct)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var content = new MultipartFormDataContent();

        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent(fid), "fid");

        await _api.UploadJournalFileAsync(content, ct);
    }

    /// <summary>
    /// Scan journal files to find the commander's FID from a Commander or LoadGame event.
    /// Reads the most recent files first for fastest discovery.
    /// </summary>
    private static string? DiscoverFID()
    {
        if (!Directory.Exists(JournalPath))
            return null;

        var logFiles = Directory.GetFiles(JournalPath, "*.log")
            .OrderByDescending(f => Path.GetFileName(f))
            .ToList();

        foreach (var filePath in logFiles)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);

                // Only need to read the first ~20 lines per file
                for (int i = 0; i < 20; i++)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        using var doc = JsonDocument.Parse(line);
                        var root = doc.RootElement;

                        if (!root.TryGetProperty("event", out var evProp))
                            continue;

                        var ev = evProp.GetString();
                        if (ev is not ("Commander" or "LoadGame"))
                            continue;

                        if (root.TryGetProperty("FID", out var fidProp))
                        {
                            var fid = fidProp.GetString();
                            if (!string.IsNullOrEmpty(fid))
                                return fid;
                        }
                    }
                    catch { /* skip bad lines */ }
                }
            }
            catch { /* skip inaccessible files */ }
        }

        return null;
    }

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
    }
}
