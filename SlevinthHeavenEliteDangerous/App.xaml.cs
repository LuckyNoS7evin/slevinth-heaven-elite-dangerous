using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SlevinthHeavenEliteDangerous.Configuration;

namespace SlevinthHeavenEliteDangerous;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private Window? _window;
    private IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
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
