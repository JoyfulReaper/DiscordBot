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

using Discord.WebSocket;
using DiscordBotLib.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLib.Helpers
{
    public static class MuteHandler
    {
        private static List<Mute> _mutes = new List<Mute>();

        public static void AddMute(Mute mute)
        {
            _mutes.Add(mute);
        }

        internal static async Task MuteWorker(DiscordSocketClient client)
        {
            List<Mute> remove = new List<Mute>();

            foreach (var mute in _mutes)
            {
                if (DateTime.Now < mute.End)
                {
                    continue;
                }

                var guild = client.GetGuild(mute.Guild.Id);

                if (guild.GetRole(mute.Role.Id) == null)
                {
                    remove.Add(mute);
                    continue;
                }

                var role = guild.GetRole(mute.Role.Id);

                if (guild.GetUser(mute.User.Id) == null)
                {
                    remove.Add(mute);
                    continue;
                }

                var user = guild.GetUser(mute.User.Id);

                if (role.Position > guild.CurrentUser.Hierarchy)
                {
                    remove.Add(mute);
                    continue;
                }

                await user.RemoveRoleAsync(mute.Role);
                remove.Add(mute);
            }

            if (remove.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var user in remove)
                {
                    sb.Append(user.User.Username);
                    sb.Append(" ");
                }
                Log.Debug("MuteHandler Unmuted: {unmutedList}", sb.ToString());
            }

            _mutes = _mutes.Except(remove).ToList();

            await Task.Delay(TimeSpan.FromMinutes(1));
            await MuteWorker(client);
        }
    }
}
