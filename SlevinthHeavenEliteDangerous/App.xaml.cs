using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SlevinthHeavenEliteDangerous.Configuration;
using System.Diagnostics;
using System.Threading;

namespace SlevinthHeavenEliteDangerous;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    private IServiceProvider? _serviceProvider;
    private static Mutex? _instanceMutex;
    private const string MutexName = "SlevinthHeavenEliteDangerous_SingleInstance_Mutex";

    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        // Check for single instance before initializing
        bool createdNew;
        _instanceMutex = new Mutex(true, MutexName, out createdNew);

        if (!createdNew)
        {
            // Another instance is already running
            Debug.WriteLine("[App] Another instance of the application is already running. Exiting.");

            // Exit the application immediately
            // In WinUI, we can't prevent the constructor from completing,
            // but we can exit in OnLaunched
            return;
        }

        InitializeComponent();

        // Configure dependency injection
        _serviceProvider = ServiceConfiguration.ConfigureServices();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        // If mutex was not created (another instance exists), exit immediately
        if (_instanceMutex == null || !_instanceMutex.WaitOne(0))
        {
            Exit();
            return;
        }

        // Window must be created in OnLaunched, not in the App constructor
        _window = _serviceProvider?.GetRequiredService<MainWindow>();
        _window?.Activate();
    }

    /// <summary>
    /// Gets the current service provider instance
    /// </summary>
    public static IServiceProvider Services => ((App)Current)._serviceProvider 
        ?? throw new InvalidOperationException("Service provider not initialized");

    /// <summary>
    /// Gets the main window instance (available after OnLaunched).
    /// </summary>
    public static MainWindow MainWindow => (MainWindow)((App)Current)._window!;
}
