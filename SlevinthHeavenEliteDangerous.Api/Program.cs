using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using SlevinthHeavenEliteDangerous.Api.Authentication;
using SlevinthHeavenEliteDangerous.Api.Components;
using SlevinthHeavenEliteDangerous.Api.Discord;
using SlevinthHeavenEliteDangerous.Api.Processing;
using SlevinthHeavenEliteDangerous.Api.Storage;
using SlevinthHeavenEliteDangerous.Eddn;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache(options => options.SizeLimit = 4096);
builder.Services.AddHttpClient("frontier-capi");

// Persist Data Protection keys so antiforgery tokens and auth cookies survive container restarts.
// Keys are stored in Data/Keys within the app content root — mount that path as a Docker volume.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "Data", "Keys")));

// Blazor Server (static SSR + interactive server for components that need it)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Cookie authentication for web sessions
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Commander data stores
builder.Services.AddSingleton<CommanderDataStore>();
builder.Services.AddSingleton<JournalFileStore>();

// EDDN
builder.Services.Configure<EddnOptions>(builder.Configuration.GetSection("Eddn"));
builder.Services.PostConfigure<EddnOptions>(o =>
    o.StoragePath = Path.Combine(builder.Environment.ContentRootPath, "Data", "Journals"));
builder.Services.AddHttpClient("eddn");
builder.Services.AddHttpClient("edsm");
builder.Services.AddSingleton<EddnSender>();
builder.Services.AddSingleton<EddnSystemLookupService>();
builder.Services.AddSingleton<EddnPublisherService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<EddnPublisherService>());

// Background journal processing
builder.Services.AddHostedService<JournalProcessingService>();

// Discord bot
builder.Services.AddSingleton(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.AllUnprivileged,
    LogLevel = LogSeverity.Info
});
builder.Services.AddSingleton<DiscordSocketClient>();
builder.Services.AddSingleton(sp => new InteractionService(
    sp.GetRequiredService<DiscordSocketClient>(),
    new InteractionServiceConfig { LogLevel = LogSeverity.Info }));
builder.Services.AddSingleton<InteractionHandler>();
builder.Services.AddSingleton<IDiscordDiagnosticsService, DiscordDiagnosticsService>();
builder.Services.AddHostedService<DiscordBotService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseMiddleware<FrontierTokenAuthMiddleware>();

app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
