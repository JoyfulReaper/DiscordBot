/*
MIT License

Copyright(c) 2022 Kyle Givler
https://github.com/JoyfulReaper

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotLibrary.Extensions;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Interactions.SlashCommands.Moderation;

[Group("mod", "Moderation commands")]
public class ModerationModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildService _guildService;
    private readonly ILogger<ModerationModule> _logger;

    public ModerationModule(IGuildService guildService,
        ILogger<ModerationModule> logger)
    {
        _guildService = guildService;
        _logger = logger;
    }

    [SlashCommand("ban", "Ban a user")]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireContext(ContextType.Guild)]
    public async Task Ban(IUser user, string? reason, int? pruneDays)
    {
        await Context.Channel.TriggerTypingAsync();

        var guildUser = user as SocketGuildUser;
        if (guildUser == null)
        {
            await RespondAsync("You can only ban guild users....");
            _logger.LogWarning("BanUser() tried to Ban an IUser that wasn't an IGuildUser. This should not happen!!!!");
            return;
        }

        await RespondAsync(embed: EmbedHelper.GetEmbed("Ban Hammer", $"{guildUser.GetDisplayName()} has been banned for *{(reason ?? "no reason")}*",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.BAN_IMAGES))));

        await _guildService.SendLogsAsync(Context.Guild, "User banned",
            $"{Context.User.Mention} has banned {user.GetDisplayName()} and deleted the past {pruneDays} day of their messages! Banned for *{(reason ?? "no reason")}*");

        await guildUser.BanAsync(pruneDays.HasValue ? pruneDays.Value : 0, reason);
    }

    [UserCommand("ban")]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireContext(ContextType.Guild)]
    public async Task BanUser(IUser user)
    {
        var guildUser = user as SocketGuildUser;
        if (guildUser == null)
        {
            await RespondAsync("You can only ban guild users....");
            _logger.LogWarning("BanUser() tried to Ban an IUser that wasn't an IGuildUser. This should not happen!!!!");
            return;
        }

        await _guildService.SendLogsAsync(Context.Guild, "User banned", $"{Context.User.Mention} has banned {user.GetDisplayName()} using the UserCommand!");
        await guildUser.BanAsync();
    }

    [SlashCommand("kick", "Kick a user")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    [RequireContext(ContextType.Guild)]
    public async Task Kick(IUser user, string? reason)
    {
        await Context.Channel.TriggerTypingAsync();

        var guildUser = user as SocketGuildUser;
        if (guildUser == null)
        {
            await RespondAsync("You can only kick guild users....");
            _logger.LogWarning("KickUser() tried to Kick an IUser that wasn't an IGuildUser. This should not happen!!!!");
            return;
        }
        await guildUser.KickAsync();

        await RespondAsync(embed: EmbedHelper.GetEmbed("Kicked", $"{guildUser.GetDisplayName()} has been kicked to the curb for *{(reason ?? "no reason")}*",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.KICK_IMAGES))));

        await _guildService.SendLogsAsync(Context.Guild, "User kicked", $"{Context.User.Mention} has kicked {user.GetDisplayName()} for *{(reason ?? "no reason")}*");
    }

    [SlashCommand("purge", "Purge messages")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [RequireBotPermission(GuildPermission.ManageMessages)]
    [RequireContext(ContextType.Guild)]
    public async Task Purge(int number)
    {
        await Context.Channel.TriggerTypingAsync();

        var messages = await Context.Channel.GetMessagesAsync(number)
            .FlattenAsync();

        var socketTextChannel = Context.Channel as SocketTextChannel;
        if (socketTextChannel == null)
        {
            await RespondAsync("You can only purge text channels....");
            _logger.LogWarning("Purge() tried to Purge a non-text channel. This should not happen!!!!");
            return;
        }
        
        await socketTextChannel.DeleteMessagesAsync(messages);
        await _guildService.SendLogsAsync(Context.Guild, "Messages Purged", $"{Context.User.Mention} purged {messages.Count()} messages in {Context.Channel}");

        await RespondAsync(embed: EmbedHelper.GetEmbed("Messages Purged", $"{Context.User.Mention} has purged {messages.Count()} messages.",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.PURGE_IMAGES))));
    }
}
