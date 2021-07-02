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
using DiscordBotLib.Helpers;
using DiscordBotLib.Services;
using DiscordBotLib.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http.Json;
using DiscordBotLib.Models.GiphyModels;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DiscordBot.Commands
{
    [Name("Fun")]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        private static readonly List<string> _eightBallResponses = new List<string>
        {
            "It is Certian.", "It is decidedly so.", "Without a doubt.", "Yes definitely.", "You may rely on it.",
            "As I see it, yes.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.",
            "Reply hazy, try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.",
            "Don't count on it.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Very doubtful."
        };
        private readonly Random _random = new();
        private readonly ILogger<FunModule> _logger;
        private readonly IServerService _servers;
        private readonly IConfiguration _config;

        public FunModule(ILogger<FunModule> logger,
            IServerService servers,
            IConfiguration config)
        {
            _logger = logger;
            _servers = servers;
            _config = config;
        }

        [Command("giphy")]
        [Summary("Search giphy for some gifs!")]
        public async Task Giphy([Summary("What to search giphy for")][Remainder]string search)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed giphy ({search}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, search, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var apiKey = _config.GetSection("GiphyApiKey").Value;

            if(String.IsNullOrWhiteSpace(apiKey))
            {
                await ReplyAsync("The api key is not correctly set in appsettings.json :(");
                return;
            }

            var uri = new Uri($"https://api.giphy.com/v1/gifs/search?api_key={apiKey}&q={search}&limit=25&offset=0&rating=pg-13&lang=en");

            var response = await HttpClientHelper.HttpClient.GetFromJsonAsync<GiphyRoot>(uri);
            await Context.Channel.SendFileAsync(Directory.GetCurrentDirectory() + @$"{Path.DirectorySeparatorChar}images{Path.DirectorySeparatorChar}Poweredby_100px-Black_VertLogo.png");
            await ReplyAsync(response.data.RandomItem().embed_url);
        }

        [Command("rockpaperscissors")]
        [Alias("rps")]
        [Summary("Play a game of rock paper scissors!")]
        public async Task RockPaperScissors()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed rockpaperscissors on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            IEmote[] rpsReactions = new IEmote[]
            {
                new Emoji("🪨"),
                new Emoji("🧻"),
                new Emoji("✂️"),
                new Emoji("❗"),
            };

            var message = await ReplyAsync("Choose Rock, Paper, or Scissors!");
            await message.AddReactionsAsync(rpsReactions);
        }

        [Command("8ball")]
        [Alias("eightBall", "8b")]
        [Summary("Ask the 8ball a question, get the answer!")]
        public async Task EightBall([Summary("The question to ask")][Remainder] string question)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed 8ball ({question}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, question, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var server = await _servers.GetServer(Context.Guild);

            var builder = new EmbedBuilder();
            builder
                .WithTitle("Magic 8ball")
                .WithThumbnailUrl(ImageLookupUtility.GetImageUrl("EIGHTBALL_IMAGES"))
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
            if (_random.Next(2) == 1)
            {
                outcome = "heads";
            }

            var server = await _servers.GetServer(Context.Guild);

            await Context.Channel.SendEmbedAsync("Coin flip", $"The coin landed {outcome} up.",
                server == null ? ColorHelper.RandomColor() : server.EmbedColor, ImageLookupUtility.GetImageUrl("COIN_IMAGES"));
        }

        [Command("rolldie")]
        [Alias("dice", "die")]
        [Summary("Roll a die")]
        public async Task CoinFlip([Summary("The number of die to roll")] int die = 1, [Summary("The number of side the die has")] int sides = 6)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed rolldie ({sides}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, sides, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if (die > 10)
            {
                await ReplyAsync("You cannot roll more than 10 die at a time. 😭");
                return;
            }

            if (sides > 25)
            {
                await ReplyAsync("Your die can't have more than 25 sides. 😭");
                return;
            }

            int sum = 0;
            for (int i = 0; i < die; i++)
            {
                sum += _random.Next(1, sides + 1);
            }

            await Context.Channel.SendEmbedAsync($"{die} die with {sides} Sides Rolled", $"🎲 You rolled: {sum} 🎲",
                ColorHelper.GetColor(await _servers.GetServer(Context.Guild)), ImageLookupUtility.GetImageUrl("DIE_IMAGES"));
        }

        [Command("RussianRoulette")]
        [Alias("rr")]
        [Summary("Are you feeling lucky punk?")]
        public async Task RussianRoulette()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed RussianRoulette on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var chamberWithBullet = _random.Next(1, 7);
            var activeChamber = _random.Next(1, 7);

            var message = "Click! Nothing happened...";
            if (activeChamber == chamberWithBullet)
            {
                message = "🔫 The revolver fires 🔫. Your brains leak out of your ears :( 🧠👂";
            }

            await Context.Channel.SendEmbedAsync("Russian Roulette", $"You spin the chamber then you pull the trigger:\n{message}",
                ColorHelper.GetColor(await _servers.GetServer(Context.Guild)), ImageLookupUtility.GetImageUrl("GUN_IMAGES"));
        }

        [Command("lmgtfy")]
        [Summary("Ask google, not me")]
        public async Task LetMeGoogleThat([Summary("What to google")][Remainder] string query = null)
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

        [Command("random")]
        [Summary("Provide a comma seperated list of items, receive a random response!")]
        public async Task RandomItem([Summary("Comma seperated list")][Remainder]string items)
        {
            var itemArr = items.Split(",", StringSplitOptions.None);

            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed random ({items}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, items, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var chosenOne = itemArr.RandomItem();

            await Context.Channel.SendEmbedAsync("Random Result", $"I have chosen: {chosenOne}", await _servers.GetServer(Context.Guild), ImageLookupUtility.GetImageUrl("DIE_IMAGES"));
        }
    }
}
