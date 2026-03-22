using SlevinthHeavenEliteDangerous.Services;
using Microsoft.UI.Dispatching;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous.ViewModels;

public sealed class FrontierAuthViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly FrontierAuthService _service;
    private readonly DispatcherQueue _dispatcherQueue;

    private bool _isAuthenticated;
    private bool _isBusy;
    private string _status = "Not logged in";
    private string? _commanderName;

    public event PropertyChangedEventHandler? PropertyChanged;

    public FrontierAuthViewModel(DispatcherQueue dispatcherQueue, FrontierAuthService service)
    {
        _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        _service = service ?? throw new ArgumentNullException(nameof(service));

        _service.AuthStateChanged += OnAuthStateChanged;
        SyncFromService();
    }

    public void Dispose() => _service.AuthStateChanged -= OnAuthStateChanged;

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        private set { if (_isAuthenticated != value) { _isAuthenticated = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotAuthenticated)); } }
    }

    public bool IsNotAuthenticated => !IsAuthenticated;

    public bool IsBusy
    {
        get => _isBusy;
        private set { if (_isBusy != value) { _isBusy = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotBusy)); } }
    }

    public bool IsNotBusy => !IsBusy;

    public string Status
    {
        get => _status;
        private set { if (_status != value) { _status = value; OnPropertyChanged(); } }
    }

    public string? CommanderName
    {
        get => _commanderName;
        private set { if (_commanderName != value) { _commanderName = value; OnPropertyChanged(); OnPropertyChanged(nameof(CommanderNameFormatted)); } }
    }

    public string CommanderNameFormatted => CommanderName is not null ? $"CMDR {CommanderName}" : string.Empty;

    // -------------------------------------------------------------------------
    // Commands (called from code-behind)
    // -------------------------------------------------------------------------

    public async Task LoginAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        Status = "Waiting for browser login...";

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        try
        {
            await _service.LoginAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            Status = "Login timed out.";
        }
        catch (InvalidOperationException ex)
        {
            Status = $"Configuration error: {ex.Message}";
        }
        catch (Exception ex)
        {
            Status = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void Logout()
    {
        _service.Logout();
        // AuthStateChanged will update the UI
    }

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------

    private void OnAuthStateChanged(object? sender, EventArgs e)
    {
        _dispatcherQueue.TryEnqueue(SyncFromService);
    }

    private void SyncFromService()
    {
        IsAuthenticated = _service.IsAuthenticated;
        CommanderName   = _service.CommanderName;
        Status = _service.IsAuthenticated
            ? $"Logged in as CMDR {_service.CommanderName ?? "Unknown"}"
            : "Not logged in";
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
