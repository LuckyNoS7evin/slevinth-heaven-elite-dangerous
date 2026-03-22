using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SlevinthHeavenEliteDangerous.Api.Discord;

/// <summary>
/// Hosted service that connects the Discord bot and keeps it alive alongside the API.
/// </summary>
public class DiscordBotService(
    DiscordSocketClient client,
    InteractionHandler interactionHandler,
    IConfiguration config,
    ILogger<DiscordBotService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = config["Discord:BotToken"];
        if (string.IsNullOrWhiteSpace(token) || token == "YOUR_BOT_TOKEN_HERE")
        {
            logger.LogWarning("Discord:BotToken is not configured. Discord bot will not start.");
            return;
        }

        client.Log += OnLogAsync;

        await interactionHandler.InitializeAsync();

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Keep running until the host shuts down
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — not an error
        }
        finally
        {
            await client.StopAsync();
            await client.LogoutAsync();
        }
    }

    private Task OnLogAsync(LogMessage log)
    {
        var level = log.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error    => LogLevel.Error,
            LogSeverity.Warning  => LogLevel.Warning,
            LogSeverity.Info     => LogLevel.Information,
            LogSeverity.Verbose  => LogLevel.Debug,
            LogSeverity.Debug    => LogLevel.Trace,
            _                    => LogLevel.Information
        };

        logger.Log(level, log.Exception, "[Discord/{Source}] {Message}", log.Source, log.Message);
        return Task.CompletedTask;
    }
}
