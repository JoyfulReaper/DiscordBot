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
using DiscordBot.Helpers;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [Name("EasterEggsHidden")]
    public class EasterEggs : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<EasterEggs> _logger;

        public EasterEggs(ILogger<EasterEggs> logger)
        {
            _logger = logger;
        }

        [Command("shelly")]
        [Summary("A picture of the bot programmer's dog")]
        [Alias("dog")]
        public async Task Shelly()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed shelly on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            EmbedBuilder builder = new EmbedBuilder();
            builder
                .WithTitle("JoyfulReaper's dog")
                .WithDescription("A picture of the DiscordBot programmer's dog")
                .WithImageUrl("https://kgivler.com/images/Shelly/Shelly.jpg")
                .WithColor(ColorHelper.RandomColor())
                .WithCurrentTimestamp();

            await ReplyAsync(null, false, builder.Build());
        }
    }
}
