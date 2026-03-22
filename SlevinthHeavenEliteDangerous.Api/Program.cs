using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.AspNetCore.Authentication.Cookies;
using SlevinthHeavenEliteDangerous.Api.Authentication;
using SlevinthHeavenEliteDangerous.Api.Components;
using SlevinthHeavenEliteDangerous.Api.Discord;
using SlevinthHeavenEliteDangerous.Api.Processing;
using SlevinthHeavenEliteDangerous.Api.Storage;
using SlevinthHeavenEliteDangerous.Eddn;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("frontier-capi");

// Blazor Server (static SSR)
builder.Services.AddRazorComponents();

// Cookie authentication for web sessions
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
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
app.MapRazorComponents<App>();

app.Run();
