using Microsoft.Extensions.DependencyInjection;
using Refit;
using SlevinthHeavenEliteDangerous.DataStorage.Services;
using SlevinthHeavenEliteDangerous.Services;
using SlevinthHeavenEliteDangerous.VoCore;
using System;

namespace SlevinthHeavenEliteDangerous.Configuration;

/// <summary>
/// Configures dependency injection services for the application
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Configure all application services
    /// </summary>
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register Services as Singletons (live for application lifetime)
        //services.AddSingleton<VoCoreDisplayService>();
        services.AddSingleton<JournalEventService>();
        services.AddSingleton<ExoBioService>();
        services.AddSingleton<VisitedSystemsService>();
        services.AddSingleton<FSDService>();
        services.AddSingleton<RankService>();
        services.AddSingleton<OverlayLogService>();
        services.AddSingleton<CommanderStatsService>();
        services.AddSingleton<ReputationService>();
        services.AddSingleton<CodexService>();
        services.AddSingleton<ApiConfigService>();
        services.AddSingleton<FrontierAuthDataService>();
        services.AddSingleton<FrontierAuthService>();

        // Journal and companion upload services
        services.AddSingleton<JournalUploadService>();
        services.AddSingleton<CompanionUploadService>();

        // Register the Frontier Bearer token handler and Refit API client
        services.AddTransient<FrontierAuthHandler>();
        services.AddRefitClient<ISlevinthHeavenApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(AppResources.ApiBaseUrl))
            .AddHttpMessageHandler<FrontierAuthHandler>();


        services.AddSingleton<VoCoreDisplayService>();

        // Register Startup Service as Singleton
        services.AddSingleton<IStartupService, StartupService>();

        // Register MainWindow as Transient (new instance each time, though typically only created once)
        services.AddTransient<MainWindow>();

        return services.BuildServiceProvider();
    }
}
