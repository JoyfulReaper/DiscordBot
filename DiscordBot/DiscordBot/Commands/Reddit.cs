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
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Discord;
using System.Collections.Generic;
using DiscordBot.Helpers;
using Microsoft.Extensions.Configuration;
using DiscordBot.DataAccess;
using DiscordBot.Models;
using System.Linq;

namespace DiscordBot.Commands
{
    public class Reddit : ModuleBase<SocketCommandContext>
    {
        // Allow the bot to learn new subbreddit for it's list of random subreddits
        private const bool _allowLearning = true;

        private readonly ILogger<Reddit> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISubredditRepository _subredditRepository;
        private readonly Random _random = new();

        private static readonly List<string> _seedSubreddits = new List<string>() {"funny", "programmerhumor", "memes", "4PanelCringe", "AdviceAnimals",
            "ATAAE", "ATBGE", "badcode", "BikiniBottomTwitter", "bitchimabus", "blackmagicfuckery", "cringe", "cringetopia",
            "eyebleach", "facepalm", "facebookcringe", "forwardsfromgrandma", "FuckNestle", "interestingasfuck",
            "nextfuckinglevel", "ProgrammerDadJokes", "programming_memes", "programminghorror", "programminghumor",
            "programmingpuns", "rareinsults", "shittyprogramming", "shittyrobots", "softwaregore", "programmingmemes",
            "whitepeopletwitter", "blackpeopletwitter", "whitepeoplegifs", "idiotsincars", "natureisfuckinglit", "dankmemes",
            "itookapicture", "catsinsinks", "animalsbeingderps", "acab", "badfaketexts", "abandonedporn", "chihuahua", "chemicalreactiongifs",
            "shittyfoodporn", "animalsbeingjerks"};

        public Reddit(ILogger<Reddit> logger, 
            IConfiguration configuration,
            ISubredditRepository subredditRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _subredditRepository = subredditRepository;
        }

        [Command("reddit", RunMode = RunMode.Async)]
        public async Task RedditPost(string subreddit = null)
        {
            _logger.LogInformation("{username}#{discriminator} invoked reddit with subreddit {subreddit}", Context.User.Username, Context.User.Discriminator, subreddit);
            await Context.Channel.TriggerTypingAsync();

            SocketTextChannel channel = Context.Channel as SocketTextChannel;
            if(channel == null)
            {
                _logger.LogWarning("Channel {channel} is not a text channel.", channel);
                return;
            }

            var subreddits = await GetSubreddits();
            if (subreddit == null)
            {
                subreddit = subreddits[_random.Next(subreddits.Count)].Name;
            }

            await AddSubRedditIfNotKnownAndLearningEnabled(subreddits, subreddit);

            HttpClient httpClient = new HttpClient();
            var httpResult = await httpClient.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");

            bool showNSFW = channel.IsNsfw;
            if(httpResult.Contains("nsfw") && showNSFW != true)
            {
                await ReplyAsync("NSFW Posts only shown on NSFW channels");
                return;
            }

            await RemoveSubredditIfNonexistant(httpResult, subreddit, subreddits);

            JArray arr = JArray.Parse(httpResult);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            await CreateAndSendEmbed(post, subreddit);
        }

        private async Task CreateAndSendEmbed(JObject post, string subreddit)
        {
            string postUrl = post["url"].ToString();
            string postTitle = post["title"].ToString();

            if (postTitle.Length >= 255)
            {
                _logger.LogWarning("reddit: Title over 256 characters, trimming!");
                postTitle = postTitle.Substring(0, 255);
            }

            var builder = new EmbedBuilder();

            var postUrlLower = postUrl.ToLowerInvariant();
            // Note to self gifv doesn't work don't add it back..
            if (postUrlLower.EndsWith("jpg") || postUrl.EndsWith("png") || postUrl.EndsWith("gif")
                || postUrl.EndsWith("bmp"))
            {
                builder.WithImageUrl(postUrl);
            }

            builder
                .WithDescription($"/r/{subreddit}")
                .AddField("url:", postUrl.ToString(), true)
                .WithColor(ColorHelper.GetColor())
                .WithTitle(postTitle.ToString())
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter($"🗨 {post["num_comments"]} ⬆️ {post["ups"]}")
                .WithCurrentTimestamp();

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        private async Task RemoveSubredditIfNonexistant(string httpResult, string subreddit, List<Subreddit> subreddits)
        {
            if (!httpResult.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync($"{subreddit} does not exist!");

                if (subreddits.Any(x => x.Name == subreddit))
                {
                    var subredditToDelete = subreddits.Where(x => x.Name == subreddit).First();
                    subreddits.Remove(subredditToDelete);
                    await _subredditRepository.DeleteAsync(subredditToDelete);
                    _logger.LogDebug("reddit: Removed {subreddit}", subreddit);
                }
                return;
            }
        }

        private async Task AddSubRedditIfNotKnownAndLearningEnabled(List<Subreddit> subreddits, string subreddit)
        {
            if (!subreddits.Any(x => x.Name == subreddit) && _allowLearning)
            {
                await _subredditRepository.AddAsync(new Subreddit { Name = subreddit, ServerId = Context.Guild.Id });

                _logger.LogInformation("reddit: learned {subreddit}", subreddit);
                Console.WriteLine("reddit: Learned" + subreddit);

                await ReplyAsync($"I learned a new subreddit! Now I know of {subreddits.Count + 1} subreddits!");
            }
        }

        private async Task<List<Subreddit>> GetSubreddits( )
        {
            var subredits = await _subredditRepository.GetSubredditByServerId(Context.Guild.Id);

            if(subredits.Count == 0)
            {
                foreach(string subreddit in _seedSubreddits)
                {
                    Subreddit sub = new Subreddit { Name = subreddit, ServerId = Context.Guild.Id };
                    subredits.Add(sub);
                    await _subredditRepository.AddAsync(sub);
                }
            }

            return subredits;
        }
    }
}
