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

using Discord.Commands;
using Discord.WebSocket;
using DiscordBotLib.DataAccess;
using DiscordBotLib.Helpers;
using DiscordBotLib.Models.DatabaseEntities;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands.Moderation
{
    [Name("Invite")]
    [Group("invite")]
    public class InviteModule : ModuleBase<SocketCommandContext>
    {
        private readonly IInviteRepository _inviteRepository;
        private readonly ILogger<InviteModule> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IServerRepository _serverRepository;
        private readonly IServerService _serverService;
        private readonly CommandHandler _commandHandler;

        public InviteModule(IInviteRepository inviteRepository,
            ILogger<InviteModule> logger,
            IUserRepository userRepository,
            IServerRepository serverRepository,
            IServerService serverService,
            CommandHandler commandHandler)
        {
            _inviteRepository = inviteRepository;
            _logger = logger;
            _userRepository = userRepository;
            _serverRepository = serverRepository;
            _serverService = serverService;
            _commandHandler = commandHandler;
        }

        [Command("status")]
        [Summary("invite tracking status")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Status()
        {
            await Context.Channel.TriggerTypingAsync();

            if (await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            _logger.LogInformation("{user}#{discriminator} invoked invite status in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, Context.Channel.Name, Context.Guild?.Name ?? "DM");

            var server = await ServerHelper.GetOrAddServer(Context.Guild.Id, _serverRepository);

            await ReplyAsync($"Invite tracking is `{(server.TrackInvites ? "Enabled" : "Disabled")}`");
        }

        [Command("enable")]
        [Summary("Enable invite tracking")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task Enable()
        {
            await Context.Channel.TriggerTypingAsync();

            if (await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            _logger.LogInformation("{user}#{discriminator} invoked invite enable in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, Context.Channel.Name, Context.Guild?.Name ?? "DM");

            var server = await ServerHelper.GetOrAddServer(Context.Guild.Id, _serverRepository);

            server.TrackInvites = true;
            await _serverRepository.EditAsync(server);

            await _commandHandler.RequestInviteUpdate();

            await ReplyAsync("Invite tracking enabled");
            await _serverService.SendLogsAsync(Context.Guild, "Invite Tracking Enabled", $"{Context.User.Mention} Enabled invite tracking!");
        }

        [Command("disable")]
        [Summary("disable invite tracking")]
        public async Task Disable()
        {
            await Context.Channel.TriggerTypingAsync();

            if (await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            _logger.LogInformation("{user}#{discriminator} invoked invite disable in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, Context.Channel.Name, Context.Guild?.Name ?? "DM");

            var server = await ServerHelper.GetOrAddServer(Context.Guild.Id, _serverRepository);

            server.TrackInvites = false;
            await _serverRepository.EditAsync(server);


            await ReplyAsync("Invite tracking disabled");
            await _serverService.SendLogsAsync(Context.Guild, "Invite Tracking Disabled", $"{Context.User.Mention} Disabled invite tracking!");
        }

        [Command("count")]
        [Summary("Get the number of users successfully invited by a given user")]
        public async Task InviteCount(
        [Summary("The user to ban")] SocketGuildUser user = null)
        {
            await Context.Channel.TriggerTypingAsync();

            if(await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            if (user == null)
            {
                //await ReplyAsync("Please provide a user to lookup!");
                user = Context.User as SocketGuildUser;
            }

            _logger.LogInformation("{user}#{discriminator} invoked invite {user} in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, user.Username, Context.Channel.Name, Context.Guild?.Name ?? "DM");

            var server = await ServerHelper.GetOrAddServer(Context.Guild.Id, _serverRepository);
            var dbuser = await UserHelper.GetOrAddUser(user, _userRepository);
            var invite = await _inviteRepository.GetInviteByUser(dbuser.Id, server.Id);
            if(invite == null)
            {
                await ReplyAsync($"{user.Username} has not invited anyone");
                return;
            }

            await ReplyAsync($"{user.Username} has invited {invite.Count} users");
        }
    }
}
