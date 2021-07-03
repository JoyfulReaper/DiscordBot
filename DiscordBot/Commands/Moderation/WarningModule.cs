/*
MIT License

Copyright(c) 2021 Kyle Givler
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
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotLib.DataAccess;
using DiscordBotLib.Enums;
using DiscordBotLib.Helpers;
using DiscordBotLib.Models;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Commands.Moderation
{
    [Name("Warning")]
    [Group("warn")]
    public class WarningModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<WarningModule> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IServerRepository _serverRepository;
        private readonly IWarningRepository _warningRepository;
        private readonly DiscordSocketClient _client;
        private readonly IServerService _servers;

        public WarningModule(ILogger<WarningModule> logger,
            IUserRepository userRepository,
            IServerRepository serverRepository,
            IWarningRepository warningRepository,
            DiscordSocketClient client,
            IServerService servers)
        {
            _logger = logger;
            _userRepository = userRepository;
            _serverRepository = serverRepository;
            _warningRepository = warningRepository;
            _client = client;
            _servers = servers;
        }

        [Command("get")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Summary("Get a users warnings")]
        [Alias("getwarns")]
        public async Task GetWarnings(SocketGuildUser user)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{user}#{discriminator} invoked getwarnings ({user}) in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, user.Username, user.Username, Context.Channel.Name, Context.Guild?.Name ?? "DM");

            var userDb = await UserHelper.GetOrAddUser(user, _userRepository);
            var server = await ServerHelper.GetOrAddServer(Context.Guild.Id, _serverRepository);
            var warnings = await _warningRepository.GetUsersWarnings(server, userDb);

            if (warnings.Count() < 1)
            {
                await ReplyAsync($"{user.Username} has not been warned!");
                return;
            }

            var warnNum = 1;
            var message = $"{user.Username} has been warned for:\n";
            foreach (var w in warnings)
            {
                message += $"{warnNum++}) {w.Text}\n";
            }

            await ReplyAsync(message);
        }

        [Command("")]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [Summary("Warn a user")]
        public async Task Warn([Summary("The user to warn")] SocketGuildUser user, [Summary("The reason for the warning")][Remainder] string reason)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{user}#{discriminator} warned {user} for {reason} in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, user.Username, reason, Context.Channel.Name, Context.Guild?.Name ?? "DM");

            if (user.Id == _client.CurrentUser.Id)
            {
                await ReplyAsync("Nice try, but I am immune from warnings!");
                return;
            }

            if (user.Id == Context.User.Id)
            {
                await ReplyAsync("Lol, you are warning yourself!");
            }

            var server = await ServerHelper.GetOrAddServer(Context.Guild.Id, _serverRepository);
            var userDb = await UserHelper.GetOrAddUser(user, _userRepository);

            var warning = new Warning
            {
                UserId = userDb.Id,
                ServerId = server.Id,
                Text = reason
            };
            await _warningRepository.AddAsync(warning);

            var warn = await _warningRepository.GetUsersWarnings(server, userDb);
            var wAction = await _warningRepository.GetWarningAction(server);

            if (wAction == null)
            {
                wAction = new WarnAction
                {
                    Action = WarningAction.NoAction,
                    ActionThreshold = 1
                };
                await ReplyAsync($"{Context.User.Mention}: NOTE! The warning action has not been set!");
            }

            await Context.Channel.SendEmbedAsync("You have been warned!", $"{user.Mention} you have been warned for: `{reason}`!\n" +
                $"This is warning #`{warn.Count()}` of `{wAction.ActionThreshold}`\n" +
                $"The action is set to: { Enum.GetName(typeof(WarningAction), wAction.Action)}",
                ColorHelper.GetColor(server));

            if (warn.Count() >= wAction.ActionThreshold)
            {
                var message = $"The maximum number of warnings has been reached, because of the warn action ";
                switch (wAction.Action)
                {
                    case WarningAction.NoAction:
                        message += "nothing happens.";
                        break;
                    case WarningAction.Kick:
                        message += $"{user.Username} has been kicked.";
                        await user.KickAsync("Maximum Warnings Reached!");
                        break;
                    case WarningAction.Ban:
                        message += $"{user.Username} has been banned.";
                        await user.BanAsync(0, "Maximum Warnings Reached!");
                        break;
                    default:
                        message += "default switch statement :(";
                        break;
                }

                await ReplyAsync(message);
            }
            await _servers.SendLogsAsync(Context.Guild, $"User Warned", $"{Context.User.Mention} warned {user.Username} for: {reason}", ImageLookupUtility.GetImageUrl("LOGGING_IMAGES"));
        }

        [Command("action")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Change the warn action")]
        public async Task WarnAction([Summary("Action: none, kick or ban")] string action = null,
            [Summary("The number of warnings before the action is performed")] int maxWarns = -1)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{user}#{discriminator} invoked warnaction ({action}, {maxWarns}) messages in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, action, maxWarns, Context.Channel.Name, Context.Guild?.Name ?? "DM");

            var server = await _serverRepository.GetByServerId(Context.Guild.Id);
            if (server == null)
            {
                server = new Server { GuildId = Context.Guild.Id };
                await _serverRepository.AddAsync(server);
            }

            if (action == null && maxWarns < 0)
            {
                var wAction = await _warningRepository.GetWarningAction(server);
                if (wAction == null)
                {
                    await ReplyAsync("The warn action has not been set.");
                    return;
                }
                await Context.Channel.SendEmbedAsync("Warn Action", $"The warn action is set to: `{ Enum.GetName(typeof(WarningAction), wAction.Action)}`. The threshold is: `{wAction.ActionThreshold}`",
                    ColorHelper.GetColor(server));

                return;
            }

            var message = $"Warn action set to `{action.ToLowerInvariant()}`, Max Warnings { maxWarns} by {Context.User.Mention}";
            bool valid = false;
            WarnAction warnAction = null;

            if (action.ToLowerInvariant() == "none" && maxWarns > 0)
            {
                valid = true;
                warnAction = new WarnAction
                {
                    ServerId = server.Id,
                    Action = WarningAction.NoAction,
                    ActionThreshold = maxWarns
                };
            }
            else if (action.ToLowerInvariant() == "kick" && maxWarns > 0)
            {
                valid = true;
                warnAction = new WarnAction
                {
                    ServerId = server.Id,
                    Action = WarningAction.Kick,
                    ActionThreshold = maxWarns
                };
            }
            else if (action.ToLowerInvariant() == "ban" && maxWarns > 0)
            {
                valid = true;
                warnAction = new WarnAction
                {
                    ServerId = server.Id,
                    Action = WarningAction.Ban,
                    ActionThreshold = maxWarns
                };
            }

            if (valid)
            {
                await _warningRepository.SetWarnAction(warnAction);
                await _servers.SendLogsAsync(Context.Guild, $"Warn Action Set", message, ImageLookupUtility.GetImageUrl("LOGGING_IMAGES"));
                await Context.Channel.SendEmbedAsync("Warn Action Set", $"Warn action set to: `{action.ToLowerInvariant()}`. Threshold set to: `{maxWarns}`",
                    ColorHelper.GetColor(server));
            }
            else
            {
                await ReplyAsync("Please provide a valid option: `none`, `kick`, `ban` and positive maximum warnings.");
            }
        }
    }
}
