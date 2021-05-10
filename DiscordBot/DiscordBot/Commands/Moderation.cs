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
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<Moderation> _logger;
        private readonly IServers _servers;
        private readonly IConfiguration _configuration;
        private readonly int _prefixMaxLength;

        public Moderation(DiscordSocketClient client,
            ILogger<Moderation> logger,
            IServers servers,
            IConfiguration configuration)
        {
            _client = client;
            _logger = logger;
            _servers = servers;
            _configuration = configuration;


            var prefixConfigValue = _configuration.GetSection("PrefixMaxLength").Value;
            if (int.TryParse(prefixConfigValue, out int maxLength))
            {
                _prefixMaxLength = maxLength;
            }
            else
            {
                _prefixMaxLength = 8;
                _logger.LogError("Unable to set max prefix length, using default: {defaultValue}", _prefixMaxLength);
            }
        }

        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Purges the given number of messages from the current channel")]
        public async Task Purge([Summary("The number of message to purge")] int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendMessageAsync($"{messages.Count()} messages deleted successfuly!");
            await Task.Delay(2500);
            await message.DeleteAsync();

            _logger.LogInformation("{user}#{discriminator} purged {number} messages in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, amount, Context.Channel.Name, Context.Guild.Name);
        }

        [Command("prefix")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Change the prefix")]
        public async Task Prefix(string prefix = null)
        {
            var myPrefix = await _servers.GetGuildPrefix(Context.Guild.Id);

            if (prefix == null)
            {
                await ReplyAsync($"My prefix is: `{myPrefix}`");
                return;
            }

            if(prefix.Length > _prefixMaxLength)
            {
                await ReplyAsync("Prefix must be less than " + _prefixMaxLength + " characters.");
                return;
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await ReplyAsync($"The prefix has been modified to `{prefix}`.");
        }
    }
}
