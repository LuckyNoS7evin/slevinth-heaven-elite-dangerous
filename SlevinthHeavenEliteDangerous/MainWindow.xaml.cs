using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.ViewModels;
using SlevinthHeavenEliteDangerous.VoCore;
using System.Threading;
using System.Threading.Tasks;

namespace SlevinthHeavenEliteDangerous;

/// <summary>
/// Main window for the Elite Dangerous companion application
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly IStartupService _startupService;
    private readonly FrontierAuthService _frontierAuthService;
    private readonly ISlevinthHeavenApi _api;
    private readonly CancellationTokenSource _startupCts = new();
    private OverlayWindow? _overlayWindow;
    private readonly JournalEventService _journalEventService;
    private readonly VoCoreDisplayService? _voCoreInstance;

    // Public property for XAML binding
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(
        IStartupService startupService,
        JournalEventService journalEventService,
        FrontierAuthService frontierAuthService,
        VoCoreDisplayService voCoreDisplayService)
    {
        _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));
        _frontierAuthService = frontierAuthService ?? throw new ArgumentNullException(nameof(frontierAuthService));
        _api = App.Services.GetRequiredService<ISlevinthHeavenApi>();

        // Create ViewModel
        ViewModel = new MainWindowViewModel(DispatcherQueue, journalEventService);
        _journalEventService = journalEventService ?? throw new ArgumentNullException(nameof(journalEventService));
        _voCoreInstance = voCoreDisplayService ?? throw new ArgumentNullException(nameof(voCoreDisplayService));

        InitializeComponent();

        // Set window icon
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
        appWindow.TitleBar.IconShowOptions = Microsoft.UI.Windowing.IconShowOptions.HideIconAndSystemMenu;
        appWindow.Title = "Slevinth's Elite Dangerous Companion";

        this.Closed += OnWindowClosed;

        // Detect VoCore device and show enable button if present
        try
        {
            if (UsbDeviceDetector.IsDevicePresent())
            {
                VoCoreToggleButton.Visibility = Visibility.Visible;
            }
        }
        catch { }

        PerformStartup();
    }


    private void VoCoreToggleButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_voCoreInstance == null)
            {
                System.Diagnostics.Debug.WriteLine("VoCore service not available");
                return;
            }

            if (!_voCoreInstance.Enabled)
            {
                _voCoreInstance.StartService();
                VoCoreToggleButton.Content = "Disable VoCore";
                System.Diagnostics.Debug.WriteLine("VoCore enabled");
            }
            else
            {
                _voCoreInstance.StopService();
                VoCoreToggleButton.Content = "Enable VoCore";
                System.Diagnostics.Debug.WriteLine("VoCore disabled");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"VoCore toggle error: {ex.Message}");
        }
    }



    /// <summary>
    /// Checks the API for the required app version.
    /// Returns false and shows the blocking update screen if this build is outdated.
    /// Returns true if versions match or the check cannot be completed (API offline).
    /// </summary>
    private async Task<bool> CheckVersionAsync()
    {
        try
        {
            var versionInfo = await _api.GetVersionAsync();
            var current = typeof(MainWindow).Assembly.GetName().Version;
            if (current is null) return true;

            var currentStr = $"{current.Major}.{current.Minor}.{current.Build}";
            if (!string.IsNullOrEmpty(versionInfo.LatestVersion) &&
                currentStr != versionInfo.LatestVersion)
            {
                ViewModel.BlockForUpdate(versionInfo.LatestVersion, versionInfo.DownloadUrl);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Version check failed: {ex.Message}");
            return true; // API unreachable — allow startup
        }
    }

    private async void DownloadUpdateButton_Click(object sender, RoutedEventArgs e)
    {
        var url = ViewModel.VersionDownloadUrl;
        if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri))
            await Windows.System.Launcher.LaunchUriAsync(uri);
    }

    private void OverlayToggleButton_Click(object sender, RoutedEventArgs e)
    {
        if (_overlayWindow == null)
        {
            _overlayWindow = new OverlayWindow();
            _overlayWindow.Closed += (_, _) =>
            {
                _overlayWindow = null;
                OverlayToggleButton.Content = "Open Overlay";
            };
        }

        if (OverlayToggleButton.Content as string == "Open Overlay")
        {
            _overlayWindow.ShowOverlay();
            OverlayToggleButton.Content = "Close Overlay";
        }
        else
        {
            _overlayWindow.HideOverlay();
            OverlayToggleButton.Content = "Open Overlay";
        }
    }

    private async void PerformStartup()
    {
        var ct = _startupCts.Token;

        try
        {
            // Step 1: Attempt silent auth restore
            ViewModel.SetInitializing(true, "Checking authentication...");
            _startupService.RegisterEventHandlers();
            await _startupService.TryRestoreAuthAsync();

            // Step 2: If not authenticated, pause startup and wait for login
            if (!_frontierAuthService.IsAuthenticated)
            {
                ViewModel.SetInitializing(false);
                ViewModel.SetLoginPrompt(true);

                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                void OnAuth(object? s, EventArgs e)
                {
                    if (_frontierAuthService.IsAuthenticated)
                    {
                        _frontierAuthService.AuthStateChanged -= OnAuth;
                        tcs.TrySetResult(true);
                    }
                }
                _frontierAuthService.AuthStateChanged += OnAuth;

                await tcs.Task.WaitAsync(ct);

                ViewModel.SetLoginPrompt(false);
                ViewModel.SetInitializing(true, "Starting up...");
            }

            // Step 3: Version check — block startup if this build is outdated
            ViewModel.SetInitializing(true, "Checking for updates...");
            if (!await CheckVersionAsync())
                return;

            // Step 4: Load data and start monitoring
            _startupService.InitializationProgress += OnInitializationProgress;
            try
            {
                await _startupService.InitializeDataAsync();
            }
            finally
            {
                _startupService.InitializationProgress -= OnInitializationProgress;
            }

            _startupService.RunDiagnostics();

            try
            {
                _startupService.StartJournalMonitoring();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to start monitoring: {ex.Message}");
            }

            // Start background journal upload (fire-and-forget — resilient to errors)
            _ = _startupService.StartJournalUploadAsync();
        }
        catch (OperationCanceledException)
        {
            // Window was closed before startup completed — nothing to do
        }
        finally
        {
            ViewModel.SetInitializing(false);
        }
    }

    private void OnInitializationProgress(object? sender, InitializationProgressEventArgs e)
    {
        // Update progress on UI thread
        ViewModel.SetInitializing(
            true,
            e.Message,
            e.TotalFiles > 0 ? $"{e.ProcessedFiles}/{e.TotalFiles} files ({e.PercentComplete}%) - {e.TotalEvents:N0} events" : string.Empty);
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        try
        {
            _startupCts.Cancel();
            _overlayWindow?.Close();
            _startupService.StopJournalUpload();
            _startupService.StopJournalMonitoring();
            ViewModel?.Dispose();
            // Clean shutdown for optional VoCore
            if (_voCoreInstance != null)
            {
                try
                {
                  //  _journalEventService.UnregisterEventHandler(_voCoreInstance);
                    _voCoreInstance.Dispose();
                }
                catch { }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error during shutdown: {ex.Message}");
        }
    }
}
