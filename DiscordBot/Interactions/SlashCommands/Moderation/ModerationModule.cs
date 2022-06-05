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
using DiscordBotLib.Models;
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

    [SlashCommand("unban", "Unban a user")]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [RequireUserPermission(GuildPermission.BanMembers)]
    public async Task Unban([Summary("userSnowFlakeId")] ulong userId)
    {
        await Context.Channel.TriggerTypingAsync();
        
        await RespondAsync(embed: EmbedHelper.GetEmbed("Un-Banned", $"{Context.Guild.GetUser(userId)?.GetDisplayName() ?? userId.ToString()} has been un-banned!",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.UNBAN_IMAGES))));

        await _guildService.SendLogsAsync(Context.Guild, "Un-Banned", $"{Context.User.Mention} has un-banned {Context.Guild.GetUser(userId)?.GetDisplayName() ?? userId.ToString()}");
        await Context.Guild.RemoveBanAsync(userId);
    }

    [UserCommand("kick")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.KickMembers)]
    [RequireContext(ContextType.Guild)]
    public async Task KickUser(IUser user)
    {
        var guildUser = user as SocketGuildUser;
        if (guildUser == null)
        {
            await RespondAsync("You can only kick guild users....");
            _logger.LogWarning("KickUser() tried to Kick an IUser that wasn't an IGuildUser. This should not happen!!!!");
            return;
        }

        await _guildService.SendLogsAsync(Context.Guild, "User Kicked", $"{Context.User.Mention} has kicked {user.GetDisplayName()} using the UserCommand!");
        await guildUser.KickAsync();
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
    
    [SlashCommand("mute", "Mute a user")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    [RequireContext(ContextType.Guild)]
    public async Task Mute([Summary("user")] IUser user,
            [Summary("minutes")] int minutes = 5,
            [Summary("reason")]string? reason = null)
    {
        await Context.Channel.TriggerTypingAsync();

        var guildUser = user as SocketGuildUser;
        if (guildUser == null)
        {
            await RespondAsync("You can only mute guild users....");
            _logger.LogError("Mute() tried to Mute an IUser that wasn't an IGuildUser. This should not happen!!!!");
            return;
        }

        if (guildUser.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
        {
            await RespondAsync(embed: EmbedHelper.GetEmbed("Invalid User", "That user has a higher position than the bot!",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.ERROR_IMAGES))));

            return;
        }

        // Check for muted role, attempt to create it if it doesn't exist
        var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
        if (role == null)
        {
            role = await Context.Guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false), null, false);
        }

        if (role.Position > Context.Guild.CurrentUser.Hierarchy)
        {
            await RespondAsync(embed: EmbedHelper.GetEmbed("Invalid permissions", "The muted role has a higher position than the bot!",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.ERROR_IMAGES))));

            return;
        }

        if (guildUser.Roles.Contains(role))
        {
            await RespondAsync(embed: EmbedHelper.GetEmbed(title: "Already muted", "That user is already muted!",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.ERROR_IMAGES))));

            return;
        }

        await role.ModifyAsync(x => x.Position = Context.Guild.CurrentUser.Hierarchy);
        foreach (var channel in Context.Guild.Channels)
        {
            if (!channel.GetPermissionOverwrite(role).HasValue || channel.GetPermissionOverwrite(role)?.SendMessages == PermValue.Allow)
            {
                await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny));
            }
        }
        
        MuteHelper.AddMute(new Mute { Guild = Context.Guild, User = guildUser, End = DateTime.Now + TimeSpan.FromMinutes(minutes), Role = role });
        await guildUser.AddRoleAsync(role);
        await RespondAsync(embed: EmbedHelper.GetEmbed("Muted", $"{guildUser.GetDisplayName()} has been muted for {minutes} minutes.\nReason: {reason ?? "None"}",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.MUTE_IMAGES))));

        await _guildService.SendLogsAsync(Context.Guild, "Muted", $"{Context.User.Mention} muted {user.Mention}");
    }

    [SlashCommand("unmute", "Remove muted role from a user")]
    [RequireUserPermission(GuildPermission.KickMembers)]
    [RequireBotPermission(GuildPermission.ManageRoles)]
    [RequireContext(ContextType.Guild)]
    private async Task Unmute([Summary("User")] IUser user)
    {
        var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
        if (role == null)
        {
            await RespondAsync(embed: EmbedHelper.GetEmbed("Not Muted", "That user is not muted!",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.ERROR_IMAGES))));

            return;
        }

        if (role.Position > Context.Guild.CurrentUser.Hierarchy)
        {
            await RespondAsync(embed: EmbedHelper.GetEmbed("Invalid permissions", "the muted role has a higher position than the bot!",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.ERROR_IMAGES))));
            
            return;
        }

        var guildUser = user as SocketGuildUser;
        if (guildUser == null)
        {
            await RespondAsync("You can only un-mute guild users....");
            _logger.LogError("Mute() tried to Mute an IUser that wasn't an IGuildUser. This should not happen!!!!");
            return;
        }

        if (!guildUser.Roles.Contains(role))
        {
            await RespondAsync(embed: EmbedHelper.GetEmbed(title: "Not muted", "That user is not muted!",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.ERROR_IMAGES))));
            
            return;
        }

        await guildUser.RemoveRoleAsync(role);

        await RespondAsync(embed: EmbedHelper.GetEmbed("Unmuted", $"{guildUser.GetDisplayName()} has been unmuted!",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.UNMUTE_IMAGES))));

        await _guildService.SendLogsAsync(Context.Guild, "Unmuted", $"{Context.User.Mention} unmuted {user.GetDisplayName()}");
    }    
}
