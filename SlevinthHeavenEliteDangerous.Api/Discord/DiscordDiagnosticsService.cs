using Discord;
using Discord.WebSocket;
using SlevinthHeavenEliteDangerous.Core.Models;
using System.Text;

namespace SlevinthHeavenEliteDangerous.Api.Discord;

/// <summary>
/// Sends a <see cref="DiagnosticsReport"/> to a configured Discord channel as a series of
/// rate-limited messages covering missing events, missing properties, and serialization failures.
/// </summary>
public sealed class DiscordDiagnosticsService(
    DiscordSocketClient client,
    IConfiguration config,
    ILogger<DiscordDiagnosticsService> logger) : IDiscordDiagnosticsService
{
    // 1 100 ms between sends — safely under Discord's 5 messages / 5 s per-channel limit.
    private const int MessageDelayMs = 1100;

    // Leave headroom below Discord's 2 000 character message limit.
    private const int MaxMessageLength = 1900;

    public async Task SendReportAsync(DiagnosticsReport report, string commanderName)
    {
        var channelIdStr = config["Discord:DiagnosticsChannelId"];
        if (string.IsNullOrWhiteSpace(channelIdStr) || channelIdStr == "0")
        {
            logger.LogWarning("[Diagnostics] Discord:DiagnosticsChannelId is not configured; skipping Discord send.");
            return;
        }

        if (!ulong.TryParse(channelIdStr, out var channelId))
        {
            logger.LogWarning("[Diagnostics] Discord:DiagnosticsChannelId '{Value}' is not a valid channel ID.", channelIdStr);
            return;
        }

        if (client.ConnectionState != ConnectionState.Connected)
        {
            logger.LogWarning("[Diagnostics] Discord client is not connected; skipping Discord send.");
            return;
        }

        if (client.GetChannel(channelId) is not IMessageChannel channel)
        {
            logger.LogWarning("[Diagnostics] Channel {ChannelId} was not found or is not a text channel.", channelId);
            return;
        }

        if (report.MissingEvents.Count == 0 && report.MissingProperties.Count == 0 && report.SerializationFailures.Count == 0)
        {
            logger.LogInformation("[Diagnostics] No missing events, missing properties, or serialization failures — skipping Discord send.");
            return;
        }

        try
        {
            await SendAllMessagesAsync(channel, report, commanderName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Diagnostics] Error while sending report to Discord.");
        }
    }

    private async Task SendAllMessagesAsync(IMessageChannel channel, DiagnosticsReport report, string commanderName)
    {
        var timestamp = new DateTimeOffset(report.GeneratedAt, TimeSpan.Zero).ToUnixTimeSeconds();

        // ── Header ────────────────────────────────────────────────────────────────
        await Send(channel,
            $"📊 **Diagnostics Report** | CMDR **{commanderName}** | <t:{timestamp}:f>\n" +
            $"Missing events: **{report.MissingEvents.Count}** | " +
            $"Events with missing props: **{report.MissingProperties.Count}** | " +
            $"Events with rank props: **{report.EventsWithRankProperties.Count}** | " +
            $"Serialization failures: **{report.SerializationFailures.Count}**");

        // ── Missing events — one message per event ─────────────────────────────
        if (report.MissingEvents.Count > 0)
        {
            await Send(channel, $"⚠️ **Missing Events** ({report.MissingEvents.Count})");

            foreach (var eventName in report.MissingEvents.OrderBy(e => e))
            {
                var content = $"**`{eventName}`**";
                report.MissingEventSamples.TryGetValue(eventName, out var sample);
                await SendWithJson(channel, content, sample, $"{eventName}.json");
            }
        }

        // ── Events with missing properties — one message per event ─────────────
        if (report.MissingProperties.Count > 0)
        {
            await Send(channel, $"🔍 **Events with Missing Properties** ({report.MissingProperties.Count})");

            foreach (var (eventName, props) in report.MissingProperties.OrderBy(k => k.Key))
            {
                var propList = string.Join("\n", props.Select(p => $"  • `{p}`"));
                var noun = props.Count == 1 ? "property" : "properties";
                var content = $"**`{eventName}`** — {props.Count} missing {noun}:\n{propList}";
                report.MissingPropertySamples.TryGetValue(eventName, out var sample);
                await SendWithJson(channel, content, sample, $"{eventName}.json");
            }
        }

        if (report.SerializationFailures.Count > 0)
        {
            await Send(channel, $"🚨 **Serialization Failures** ({report.SerializationFailures.Count})");

            foreach (var failure in report.SerializationFailures
                .OrderBy(f => f.EventName)
                .ThenBy(f => f.Error))
            {
                var content =
                    $"**`{failure.EventName}`**" +
                    (string.IsNullOrWhiteSpace(failure.ClrTypeName) ? string.Empty : $" → `{failure.ClrTypeName}`") +
                    $"\nError: `{failure.Error}`" +
                    (string.IsNullOrWhiteSpace(failure.ExceptionType) ? string.Empty : $"\nException: `{failure.ExceptionType}`") +
                    (string.IsNullOrWhiteSpace(failure.SourceContext) ? string.Empty : $"\nSource: `{failure.SourceContext}`");

                await SendWithJson(
                    channel,
                    Truncate(content),
                    failure.RawJson,
                    $"{failure.EventName}-serialization-error.json");
            }
        }

        // ── Footer / summary ──────────────────────────────────────────────────
        if (report.EventsWithRankProperties.Count > 0)
        {
            var names = string.Join(", ", report.EventsWithRankProperties.Keys.OrderBy(k => k).Select(k => $"`{k}`"));
            await Send(channel, Truncate($"🏅 **Events with rank properties** ({report.EventsWithRankProperties.Count}): {names}"));
        }

        await Send(channel,
            $"📋 **Scan complete.** {report.MissingEvents.Count} missing events, {report.MissingProperties.Count} events with missing properties, {report.SerializationFailures.Count} serialization failures.");
    }

    /// <summary>
    /// Sends <paramref name="content"/> with an optional JSON sample inline. If the combined
    /// message would exceed <see cref="MaxMessageLength"/>, the JSON is uploaded as a file
    /// attachment instead of being embedded in the message text.
    /// </summary>
    private static async Task SendWithJson(IMessageChannel channel, string content, string? json, string filename)
    {
        if (string.IsNullOrEmpty(json))
        {
            await Send(channel, Truncate(content));
            return;
        }

        var inlined = $"{content}\n```json\n{json}\n```";

        if (inlined.Length <= MaxMessageLength)
        {
            await Send(channel, inlined);
        }
        else
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            await channel.SendFileAsync(stream, filename, Truncate(content));
            await Task.Delay(MessageDelayMs);
        }
    }

    private static async Task Send(IMessageChannel channel, string content)
    {
        await channel.SendMessageAsync(content);
        await Task.Delay(MessageDelayMs);
    }

    private static string Truncate(string content) =>
        content.Length > MaxMessageLength ? content[..MaxMessageLength] + "…" : content;
}

