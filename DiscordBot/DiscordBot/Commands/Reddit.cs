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

namespace DiscordBot.Commands
{
    public class Reddit : ModuleBase<SocketCommandContext>
    {
        // Allow the bot to learn new subbreddit for it's list of random subreddits
        private const bool _allowLearning = true;

        private readonly ILogger<Reddit> _logger;
        private readonly DiscordSocketClient _client;
        private readonly Random _random = new();

        // TODO store learned subbreddit in the database
        // Looks like this class is instantiated as needed, so adding is pointless at the moment...
        private static readonly List<string> _subreddits = new List<string>() {"funny", "programmerhumor", "memes", "4PanelCringe", "AdviceAnimals",
            "ATAAE", "ATBGE", "badcode", "BikiniBottomTwitter", "bitchimabus", "blackmagicfuckery", "cringe", "cringetopia",
            "eyebleach", "facepalm", "facebookcringe", "forwardsfromgrandma", "FuckNestle", "interestingasfuck",
            "nextfuckinglevel", "ProgrammerDadJokes", "programming_memes", "programminghorror", "programminghumor",
            "programmingpuns", "rareinsults", "shittyprogramming", "shittyrobots", "softwaregore", "programmingmemes"};

        public Reddit(ILogger<Reddit> logger, DiscordSocketClient client)
        {
            _logger = logger;
            _client = client;
        }

        [Command("reddit")]
        public async Task RedditPost(string subreddit = null)
        {
            _logger.LogInformation("{username}#{discriminator} invoked reddit with subreddit {subreddit}", Context.User.Username, Context.User.Discriminator, subreddit);

            if (subreddit == null)
            {
                subreddit = _subreddits[_random.Next(_subreddits.Count)];
            }
            
            if(!_subreddits.Contains(subreddit) && _allowLearning)
            {
                _subreddits.Add(subreddit);

                _logger.LogInformation("reddit: learned {subreddit}", subreddit);
                Console.WriteLine("reddit: Learned" + subreddit);

                await ReplyAsync($"I learned a new subreddit! Now I know of {_subreddits.Count} PS: learning currently doesn't work correctly...");
            }

            HttpClient httpClient = new HttpClient();
            var result = await httpClient.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");

            if(!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync($"{subreddit} does not exist!");
                if(_subreddits.Contains(subreddit))
                {
                    subreddit.Remove(subreddit.IndexOf(subreddit));
                    _logger.LogDebug("reddit: Removed {subreddit}", subreddit);
                }
                return;
            }

            JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            string postUrl = post["url"].ToString();
            string postTitle = post["title"].ToString();

            if (postTitle.Length >= 255)
            {
                _logger.LogWarning("reddit: Title over 256 characters, trimming!");
                postTitle = postTitle.Substring(0, 255);
            }

            var builder = new EmbedBuilder();

            var postUrlLower = postUrl.ToLowerInvariant();
            if (postUrlLower.EndsWith("jpg") || postUrl.EndsWith("png") || postUrl.EndsWith("gif") 
                || postUrl.EndsWith("bmp") || postUrl.EndsWith("gifv"))
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
    }
}
