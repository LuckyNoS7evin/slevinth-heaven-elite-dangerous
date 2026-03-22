using Discord;
using Discord.Interactions;

namespace SlevinthHeavenEliteDangerous.Api.Discord.Modules;

/// <summary>
/// General-purpose slash commands.
/// Add new commands here as public methods decorated with [SlashCommand].
/// </summary>
public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Check whether the bot is alive")]
    public async Task PingAsync()
    {
        var latency = Context.Client.Latency;
        await RespondAsync($"Pong! 🏓 Latency: {latency} ms");
    }

    [SlashCommand("info", "Show information about this bot")]
    public async Task InfoAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Slevinth's Elite Dangerous Companion")
            .WithDescription("Tracking exobiology discoveries, visited systems, and commander stats.")
            .WithColor(Color.Orange)
            .WithCurrentTimestamp()
            .Build();

        await RespondAsync(embed: embed);
    }
}
