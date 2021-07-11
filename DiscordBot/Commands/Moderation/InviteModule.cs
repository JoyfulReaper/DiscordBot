﻿/*
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

        public InviteModule(IInviteRepository inviteRepository,
            ILogger<InviteModule> logger,
            IUserRepository userRepository)
        {
            _inviteRepository = inviteRepository;
            _logger = logger;
            _userRepository = userRepository;
        }

        [Command("count")]
        [Summary("Get the number of users successfully invited by a given user")]
        public async Task InviteCount(
        [Summary("The user to ban")] SocketGuildUser user)
        {
            await Context.Channel.TriggerTypingAsync();

            if(await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            if (user == null)
            {
                await ReplyAsync("Please provide a user to lookup!");
            }

            _logger.LogInformation("{user}#{discriminator} invoked invite {user} in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, user.Username, Context.Channel.Name, Context.Guild?.Name ?? "DM");


        }
    }
}