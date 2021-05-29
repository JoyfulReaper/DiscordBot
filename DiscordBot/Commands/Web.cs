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
using DiscordBot.Helpers;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Web;

namespace DiscordBot.Commands
{
    public class Web : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Web> _logger;
        private readonly IServerService _servers;

        public Web(ILogger<Web> logger,
            IServerService servers)
        {
            _logger = logger;
            _servers = servers;
        }

        [Command("google")]
        [Summary("Goolge it")]
        public async Task LetMeGoogleThat([Summary("What to google")][Remainder] string query = null)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed google ({query}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, query, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var embedColor = ColorHelper.GetColor(await _servers.GetServer(Context.Guild));

            if (query == null)
            {
                await Context.Channel.SendEmbedAsync("Bad Request", "You didn't tell me what to search for!", embedColor);
                return;
            }

            var url = "https://www.google.com/search?q=" + HttpUtility.UrlEncode(query);
            await Context.Channel.SendEmbedAsync("Google Results", $"I searched google for you:\n{url}", embedColor, "https://www.computerhope.com/jargon/s/search-engine.jpg");
        }
    }
}
