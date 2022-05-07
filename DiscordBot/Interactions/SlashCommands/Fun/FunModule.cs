/*
MIT License

Copyright(c) 2022 Kyle Givler
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
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Interactions.SlashCommands.Fun.GiphyModels;
using DiscordBotLibrary.Extensions;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Infrastructure;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Web;

namespace DiscordBot.Interactions.SlashCommands.Fun;
[Group("fun", "fun commands")]
[RequireContext(ContextType.Guild)]
public class FunModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildService _guildService;
    private readonly IBannerImageService _bannerImageService;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    private static readonly Random _random = new Random();
    private static readonly List<string> _eightBallResponses = new List<string>
        {
            "It is Certian.", "It is decidedly so.", "Without a doubt.", "Yes definitely.", "You may rely on it.",
            "As I see it, yes.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.",
            "Reply hazy, try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.",
            "Don't count on it.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Very doubtful."
        };

    public FunModule(IGuildService guildService, 
        IBannerImageService bannerImageService,
        IConfiguration config,
        IHttpClientFactory httpClientFactory)
    {
        _guildService = guildService;
        _bannerImageService = bannerImageService;
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    [SlashCommand("banner", "Show the welcome banner for a given user")]
    [RequireContext(ContextType.Guild)]
    public async Task Banner(SocketGuildUser user)
    {
        await Context.Channel.TriggerTypingAsync();
        
        var background = await _guildService.GetBannerImageAsync(Context.Guild.Id);
        using var memoryStream = await _bannerImageService.CreateImage(user, background);
        memoryStream.Seek(0, SeekOrigin.Begin);
        await RespondWithFileAsync(memoryStream, $"{user.DisplayName}.png");
    }

    [SlashCommand("8ball", "Ask the 8ball a question!")]
    public async Task EightBall([Summary("question")]string question)
    {
        await Context.Channel.TriggerTypingAsync();
        
        var builder = new EmbedBuilder();
        builder
            .WithTitle("Magic 8ball")
            .WithThumbnailUrl(ImageLookup.GetImageUrl(nameof(ImageLookup.EIGHTBALL_IMAGES)))
            .WithDescription($"{Context.User.Username} asked ***{question}***")
            .AddField("Response", _eightBallResponses.RandomItem())
            .WithColor(await _guildService.GetEmbedColorAsync(Context))
            .WithCurrentTimestamp();

        await RespondAsync(embed: builder.Build());
    }

    [SlashCommand("giphy", "Search giphy!")]
    public async Task<RuntimeResult> Giphy([Summary("search")]string search)
    {
        await Context.Channel.TriggerTypingAsync();

        var apiKey = _config.GetSection("GiphyApiKey").Value;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            await ReplyAsync("The api key is not correctly set in appsettings.json :(");
            return InteractionResult.FromError(InteractionCommandError.Unsuccessful, "The giphy API key has not been set correctly!");
        }

        var uri = new Uri($"https://api.giphy.com/v1/gifs/search?api_key={apiKey}&q={search}&limit=25&offset=0&rating=pg-13&lang=en");
        var client = _httpClientFactory.CreateClient();
        var gif = await client.GetFromJsonAsync<GiphyRoot>(uri);

        if(gif == null)
        {
            return InteractionResult.FromError(InteractionCommandError.Unsuccessful, "Giphy: The API did not return a successful response!");
        }

        if (gif.data.Count == 0)
        {
            return InteractionResult.FromError(InteractionCommandError.Unsuccessful, "Giphy: No results were found!");
        }

        await RespondAsync(gif.data.RandomItem(10).embed_url);
        await Context.Channel.SendFileAsync(Directory.GetCurrentDirectory() + @$"{Path.DirectorySeparatorChar}Images{Path.DirectorySeparatorChar}Poweredby_100px-Black_VertLogo.png");

        return InteractionResult.FromSuccess();
    }

    [SlashCommand("coinflip", "Flip a coin!")]
    public async Task CoinFlip()
    {
        await Context.Channel.TriggerTypingAsync();

        string outcome = "tails";
        if (_random.Next(2) == 1)
        {
            outcome = "heads";
        }

        await RespondAsync(embed: EmbedHelper.GetEmbed("Coin Flip", $"The coin landed {outcome} up.", await _guildService.GetEmbedColorAsync(Context),
            ImageLookup.GetImageUrl(nameof(ImageLookup.COIN_IMAGES))));
    }

    [SlashCommand("rolldie", "Roll a die!")]
    public async Task CoinFlip([Summary("quantity")] int die = 1, [Summary("sides")] int sides = 6)
    {
        await Context.Channel.TriggerTypingAsync();


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

        await RespondAsync(embed: EmbedHelper.GetEmbed($"{die} die with {sides} Sides Rolled", $"🎲 You rolled: {sum} 🎲",
            await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.DIE_IMAGES))));
    }

    [SlashCommand("russianroulette", "Are you feeling lucky punk?")]
    public async Task RussianRoulette()
    {
        await Context.Channel.TriggerTypingAsync();

        var chamberWithBullet = _random.Next(1, 7);
        var activeChamber = _random.Next(1, 7);

        var message = "Click! Nothing happened...";
        if (activeChamber == chamberWithBullet)
        {
            message = "🔫 The revolver fires 🔫. Your brains leak out of your ears :( 🧠👂";
        }

        await RespondAsync(embed: EmbedHelper.GetEmbed("Russian Roulette", $"You spin the chamber then you pull the trigger:\n{message}",
            await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.GUN_IMAGES))));
    }

    [SlashCommand("lmgtfy", "Don't ask me, ask Google!")]
    public async Task LetMeGoogleThat([Summary("search")]string query)
    {
        await Context.Channel.TriggerTypingAsync();
        
        var url = "https://lmgtfy.app/?q=" + HttpUtility.UrlEncode(query);
        await RespondAsync(embed: EmbedHelper.GetEmbed("Let me Google that for you!", $"Click here: {url}", 
            await _guildService.GetEmbedColorAsync(Context)));
    }

    [SlashCommand("random", "Provide a comma seperated list of items, receive a random response!")]
    public async Task RandomItem([Summary("list")]string items)
    {
        await Context.Channel.TriggerTypingAsync();
        var itemArr = items.Split(",", StringSplitOptions.RemoveEmptyEntries);

        var chosenOne = itemArr.RandomItem();
        await RespondAsync(embed: EmbedHelper.GetEmbed("Random Result", $"The choices were: {items}\nI have chosen: {chosenOne}",
            await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.DIE_IMAGES))));
    }
}
