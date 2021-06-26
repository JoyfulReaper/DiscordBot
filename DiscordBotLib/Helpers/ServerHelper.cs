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
using DiscordBotLib.Models;
using System.Threading.Tasks;
using Serilog;
using DiscordBotLib.DataAccess;

namespace DiscordBotLib.Helpers
{
    public static class ServerHelper
    {
        public async static Task<Server> GetOrAddServer(ulong serverId, IServerRepository serverRepository)
        {
            var server = await serverRepository.GetByServerId(serverId);
            if (server == null)
            {
                server = new Server { GuildId = serverId };
                await serverRepository.AddAsync(server);
            }

            return server;
        }

        /// <summary>
        /// Check if Context is a DM
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="requireGuild">if true, reply with an error message</param>
        /// <returns>true if context is a DM, false otherwise</returns>
        public async static Task<bool> CheckIfContextIsDM(SocketCommandContext context, bool requireGuild = true)
        {
            if (context.Guild == null && requireGuild)
            {
                await context.Channel.SendEmbedAsync("You aren't in a server!", "This command only works from within a server...",
                    ColorHelper.RandomColor(), "https://3.bp.blogspot.com/-QQeOVVtLV2I/VWCVLRT7LeI/AAAAAAAABNc/9cXvdV9W7qg/s1600/dumb-face.jpg");

                return true;
            }

            if(context.Message.Channel is IDMChannel)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a message contains a server invite, and remove it if invites are disabled
        /// </summary>
        /// <param name="message">The message to check</param>
        /// <param name="server">The server the message was sent from</param>
        /// <returns></returns>
        internal static async Task CheckForServerInvites(SocketUserMessage message, Server server)
        {
            if (server == null || server.AllowInvites || message == null)
            {
                return;
            }

            var channel = message.Channel as SocketGuildChannel;
            if (message.Content.Contains("https://discord.gg/"))
            {
                if (channel.Guild.GetUser(message.Author.Id).GuildPermissions.Administrator)
                {
                    return;
                }

                await message.DeleteAsync();
                await message.Channel.SendMessageAsync($"{message.Author.Mention} You cannot send Discord Invite links!");

                Log.Information("{user} was denied posting an invite in {server}/{channel}", message.Author.Username, channel.Guild, message.Channel);
            }
        }
    }
}
