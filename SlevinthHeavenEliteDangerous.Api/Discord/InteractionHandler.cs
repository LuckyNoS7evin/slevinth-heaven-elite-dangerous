using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace SlevinthHeavenEliteDangerous.Api.Discord;

/// <summary>
/// Discovers interaction modules, registers slash commands with Discord,
/// and routes incoming interactions to the correct handler.
/// </summary>
public class InteractionHandler(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    IConfiguration config,
    ILogger<InteractionHandler> logger)
{
    public async Task InitializeAsync()
    {
        // Auto-discover all InteractionModuleBase subclasses in this assembly
        await interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), services);

        client.Ready             += OnReadyAsync;
        client.InteractionCreated += OnInteractionCreatedAsync;
        interactions.Log         += OnLogAsync;
    }

    private async Task OnReadyAsync()
    {
        var guildIdStr = config["Discord:GuildId"];

        if (ulong.TryParse(guildIdStr, out var guildId))
        {
            // Guild registration is instant — ideal for development
            await interactions.RegisterCommandsToGuildAsync(guildId);
            logger.LogInformation("Slash commands registered to guild {GuildId}", guildId);
        }
        else
        {
            // Global registration propagates within ~1 hour — use for production
            await interactions.RegisterCommandsGloballyAsync();
            logger.LogInformation("Slash commands registered globally");
        }
    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        try
        {
            var ctx = new SocketInteractionContext(client, interaction);
            var result = await interactions.ExecuteCommandAsync(ctx, services);

            if (!result.IsSuccess)
                logger.LogWarning("Interaction failed: {Error}", result.ErrorReason);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception while processing interaction");
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
            _                    => LogLevel.Debug
        };

        logger.Log(level, log.Exception, "[Interactions/{Source}] {Message}", log.Source, log.Message);
        return Task.CompletedTask;
    }
}
