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

using Discord.WebSocket;
using DiscordBotLib.DataAccess;
using DiscordBotLib.Models;
using DiscordBotLib.Models.DatabaseEntities;
using System.Threading.Tasks;

namespace DiscordBotLib.Helpers
{
    public static class UserHelper
    {
        public static async Task<DiscordBotLib.Models.User> GetOrAddUser(SocketUser user, IUserRepository userRepository)
        {
            var userDb = await userRepository.GetByUserId(user.Id);
            if (userDb == null)
            {
                userDb = new DiscordBotLib.Models.User { UserId = user.Id, UserName = user.Username };
                await userRepository.AddAsync(userDb);
            }
            if(userDb.UserName != user.Username)
            {
                userDb.UserName = user.Username;
                await userRepository.EditAsync(userDb);
            }

            return userDb;
        }

        public static async Task<Invite> GetOrAddInvite(SocketUser user, Server server, IUserRepository userRepository, IInviteRepository inviteRepository)
        {
            var dbUser = await GetOrAddUser(user, userRepository);
            var invite = await inviteRepository.GetInviteByUser(dbUser.Id, server.Id);

            if(invite == null)
            {
                invite = new Invite
                {
                    UserId = dbUser.Id,
                    Count = 0
                };

            await inviteRepository.EditAsync(invite);
            }

            return invite;
        }
    }
}
