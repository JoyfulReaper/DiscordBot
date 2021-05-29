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
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace DiscordBot.Commands
{
    public class Fun : ModuleBase<SocketCommandContext>
    {
        private static readonly List<string> _eightBallImages = new List<string>
        {
            "https://upload.wikimedia.org/wikipedia/commons/9/90/Magic8ball.jpg",
            "https://media1.tenor.com/images/821d79609a5bc1395d8dacab2ad8e8b6/tenor.gif?itemid=17798802",
            "https://media2.giphy.com/media/26xBJp4dcSdGxv2Zq/giphy.gif?cid=ecf05e47pnz54n4axc22ms430kzxmt8t1db6jm6qfm5vmg1p&rid=giphy.gif&ct=g"
        };
        private static readonly List<string> _eightBallResponses = new List<string>
        {
            "It is Certian.", "It is decidedly so.", "Without a doubt.", "Yes definitely.", "You may rely on it.",
            "As I see it, yes.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.",
            "Reply hazy, try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.",
            "Don't count on it.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Very doubtful."
        };
        private readonly Random _random = new();
        private readonly ILogger<Fun> _logger;
        private readonly IServerService _servers;

        public Fun(ILogger<Fun> logger,
            IServerService servers)
        {
            _logger = logger;
            _servers = servers;
        }

        [Command("8ball")]
        [Alias("eightBall")]
        [Summary("Ask the 8ball a question, get the answer!")]
        public async Task EightBall([Summary("The question to ask")][Remainder]string question)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed 8ball ({question}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, question, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var server = await _servers.GetServer(Context.Guild);

            var builder = new EmbedBuilder();
            builder
                .WithTitle("Magic 8ball")
                .WithThumbnailUrl(_eightBallImages.RandomItem())
                .WithDescription($"{Context.User.Username} asked ***{question}***")
                .AddField("Response", _eightBallResponses.RandomItem())
                .WithColor(server == null ? ColorHelper.RandomColor() : server.EmbedColor)
                .WithCurrentTimestamp();

            await ReplyAsync(null, false, builder.Build());
        }

        [Command("coinflip")]
        [Alias("flipcoin")]
        [Summary("Flip a coin!")]
        public async Task CoinFlip()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed coinflip on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            string outcome = "tails";
            if(_random.Next(2) == 1)
            {
                outcome = "heads";
            }

            var server = await _servers.GetServer(Context.Guild);

            await Context.Channel.SendEmbedAsync("Coin flip", $"The coin landed {outcome} up.",
                server == null ? ColorHelper.RandomColor() : server.EmbedColor, "https://www.bellevuerarecoins.com/wp-content/uploads/2013/11/bigstock-Coin-Flip-5807921.jpg");
        }

        [Command("rolldie")]
        [Summary("Roll a die")]
        public async Task CoinFlip([Summary("The number of side the die has")] int sides = 6)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed rolldie ({sides}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, sides, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var result = _random.Next(1, sides + 1);

            await Context.Channel.SendEmbedAsync($"{sides} Sided Die Roll", $"You rolled a {result}",
                ColorHelper.GetColor(await _servers.GetServer(Context.Guild)), "https://miro.medium.com/max/1920/0*bLJxMZ_YS0RxF-82.jpg");
        }

        [Command("RussianRoulette")]
        [Alias("rr")]
        [Summary("Are you feeling lucky punk?")]
        public async Task RussianRoulette()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed RussianRoulette on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var result = _random.Next(1, 7);
            var message = "Click! Nothing happened...";
            if(result == 6)
            {
                message = "🔫 The revolver fires 🔫. Your brains leak out of your ears :(";
            }

            await Context.Channel.SendEmbedAsync("Russian Roulette", $"You pull the trigger: {message}",
                ColorHelper.GetColor(await _servers.GetServer(Context.Guild)), "https://www.wealthmanagement.com/sites/wealthmanagement.com/files/styles/article_featured_standard/public/gun-one-bullet-russian-roulette.jpg?itok=Q55CNN7q");
        }

        [Command("lmgtfy")]
        [Summary("Ask google, not me")]
        public async Task LetMeGoogleThat([Summary("What to google")] [Remainder]string query = null)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed lmgtfy ({query}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, query, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var embedColor = ColorHelper.GetColor(await _servers.GetServer(Context.Guild));

            if (query == null)
            {
                await Context.Channel.SendEmbedAsync("Bad Request", "You didn't tell me what to search for!", embedColor);
                return;
            }

            var url = "https://lmgtfy.com/?q=" + HttpUtility.UrlEncode(query);
            await Context.Channel.SendEmbedAsync("Let me Google that for you", $"Here is it: {url}", embedColor);
        }
    }
}
