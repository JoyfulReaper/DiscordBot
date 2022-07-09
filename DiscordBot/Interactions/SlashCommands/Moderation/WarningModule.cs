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
using DiscordBot.Interactions.Infrastructure;
using DiscordBotLibrary.Enums;
using DiscordBotLibrary.Extensions;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Models;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Interactions.SlashCommands.Moderation;

[Group("warn", "User Warning commands")]
public class WarningModule : DiscordBotModuleBase<SocketInteractionContext>
{
    private readonly IGuildService _guildService;
    private readonly IUserService _userService;
    private readonly IWarningService _warningService;
    private readonly IDiscordClient _discordClient;
    private readonly ILogger<WarningModule> _logger;

    public WarningModule(IGuildService guildService, IUserService userService,
        IWarningService warningService, DiscordSocketClient discordClient, ILogger<WarningModule> logger) : base(guildService)
    {
        _guildService = guildService;
        _userService = userService;
        _warningService = warningService;
        _discordClient = discordClient;
        _logger = logger;
    }

    [SlashCommand("clear", "Clear a users warnings")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireContext(ContextType.Guild)]
    public async Task ClearWarnings(IUser user)
    {
        await Context.Channel.TriggerTypingAsync();

        var userDb = await _userService.LoadUserAsync(user.Id);
        var guild = await _guildService.LoadGuildAsync(Context.Guild.Id);
        await _warningService.ClearWarningsAsync(userDb.UserId, guild.GuildId);

        await RespondAsync($"Cleared all warnings for `{user.GetDisplayName()}`");
        await SendLogsAsync("Warnings Cleared", $"{Context.User.GetDisplayName()} has cleared all warnings for {user.GetDisplayName()}");
    }

    [SlashCommand("show", "Shows a users warnings")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireContext(ContextType.Guild)]
    public async Task GetWarnings(IUser user)
    {
        await Context.Channel.TriggerTypingAsync();

        var userDb = await _userService.LoadUserAsync(user.Id);
        var guild = await _guildService.LoadGuildAsync(Context.Guild.Id);
        var warnings = await _warningService.GetWarningsAsync(guild.GuildId, userDb.UserId);

        if (warnings.Count() < 1)
        {
            await RespondAsync($"{user.GetDisplayName()} has not been warned!");
            return;
        }

        var warnNum = 1;
        var message = $"{user.GetDisplayName()} has been warned for:\n";
        foreach (var w in warnings)
        {
            message += $"{warnNum++}) {w.Text}\n";
        }

        await RespondAsync(message);
    }

    [SlashCommand("warn", "Warn a user")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [RequireContext(ContextType.Guild)]
    public async Task Warn(IUser user, string? reason = null)
    {
        var guildUser = user as SocketGuildUser;
        if (guildUser == null)
        {
            await RespondAsync("Something went wrong....");
            _logger.LogWarning("Warn() tried to warn an IUser that wasn't an IGuildUser. This should not happen!!!!");
            return;
        }

        var preMessage = "";
        var postMessage = "";

        await Context.Channel.TriggerTypingAsync();

        if (guildUser.Id == _discordClient.CurrentUser.Id)
        {
            await RespondAsync("Nice try, but I am immune from warnings!");
            return;
        }

        var guild = await _guildService.LoadGuildAsync(Context.Guild.Id);
        var userDb = await _userService.LoadUserAsync(guildUser.Id);

        var warning = new Warning
        {
            UserId = userDb.UserId,
            GuildId = guild.GuildId,
            Text = reason
        };
        await _warningService.AddWarningAsync(warning);

        var warn = await _warningService.GetWarningsAsync(userDb.UserId, guild.GuildId);
        var wAction = await _warningService.GetWarningActionAsync(guild.GuildId);

        if (wAction == null)
        {
            wAction = new WarningAction
            {
                Action = WarnAction.NoAction,
                ActionThreshold = 1
            };
            postMessage += $"\n\n{Context.User.Mention}: NOTE! The warning action has not been set!";
        }

        if (guildUser.Id == Context.User.Id)
        {
            preMessage += "**Lol, you are warning yourself!**\n\n";
        }

        var actionMessage = "";
        if (warn.Count() >= wAction.ActionThreshold)
        {
            actionMessage += $"The maximum number of warnings has been reached, because of the warn action ";
            switch (wAction.Action)
            {
                case WarnAction.NoAction:
                    actionMessage += "nothing happens.";
                    break;
                case WarnAction.Kick:
                    actionMessage += $"{guildUser.GetDisplayName()} has been kicked.";
                    await guildUser.KickAsync("Maximum Warnings Reached!");
                    break;
                case WarnAction.Ban:
                    actionMessage += $"{guildUser.GetDisplayName()} has been banned.";
                    await guildUser.BanAsync(0, "Maximum Warnings Reached!");
                    break;
                default:
                    actionMessage += "default switch statement :(";
                    break;
            }
        }

        await RespondWithEmbedAsync("You have been warned!", $"{preMessage}{guildUser.Mention} you have been warned for: `{reason ?? "no reason"}`!\n" +
            $"This is warning #`{warn.Count()}` of `{wAction.ActionThreshold}`\n\n" +
            $"The action is set to: { Enum.GetName(typeof(WarnAction), wAction.Action)}" +
            $"\n\n{actionMessage}" +
            $"{postMessage}");

        await SendLogsAsync($"User Warned", $"{Context.User.Mention} warned {guildUser.GetDisplayName()} for: `{reason ?? "no reason"}`",
            ImageLookup.GetImageUrl(nameof(ImageLookup.LOGGING_IMAGES)));
    }

    [SlashCommand("action", "Change the warn action")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireBotPermission(GuildPermission.BanMembers)]
    [RequireContext(ContextType.Guild)]
    public async Task WarnActionAsync(WarnAction action, int maxWarns)
    {
        await Context.Channel.TriggerTypingAsync();

        var guild = await _guildService.LoadGuildAsync(Context.Guild.Id);

        var message = $"Warn action set to `{action}`, Max Warnings {maxWarns} by {Context.User.GetDisplayName()}";
        WarningAction warningAction = new WarningAction()
        {
            GuildId = guild.GuildId,
            Action = action,
            ActionThreshold = maxWarns
        };
            await _warningService.SetWarningActionAsync(warningAction);
            await SendLogsAsync("Warn Action Set", message, ImageLookup.GetImageUrl("LOGGING_IMAGES"));
            await RespondWithEmbedAsync("Warn Action Set", $"Warn action set to: `{action}`. Threshold set to: `{maxWarns}`");
    }

    [SlashCommand("displayaction", "Display warning action")]
    [RequireContext(ContextType.Guild)]
    public async Task GetWarningActionAsync()
    {
        await Context.Channel.TriggerTypingAsync();

        var guild = await _guildService.LoadGuildAsync(Context.Guild.Id);
        var warningAction = await _warningService.GetWarningActionAsync(guild.GuildId);

        await RespondWithEmbedAsync("Warning Action", $"Warning Action is currently set to: `{warningAction!.Action}`. Threshold set to: `{warningAction.ActionThreshold}`");
    }
}