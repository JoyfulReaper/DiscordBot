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
using DiscordBot.Attributes;
using DiscordBot.Services;

namespace DiscordBot.Commands
{
    public class Reddit : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<Reddit> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISubredditRepository _subredditRepository;
        private readonly IServerRepository _serverRepository;
        private readonly IServerService _servers;
        private readonly Random _random = new();

        private static readonly List<string> _seedSubreddits = new List<string>() {"funny", "programmerhumor", "memes", "4PanelCringe", "AdviceAnimals",
            "ATAAE", "ATBGE", "badcode", "BikiniBottomTwitter", "bitchimabus", "blackmagicfuckery", "cringe", "cringetopia",
            "eyebleach", "facepalm", "facebookcringe", "forwardsfromgrandma", "FuckNestle", "interestingasfuck", "bloop",
            "nextfuckinglevel", "ProgrammerDadJokes", "programming_memes", "programminghorror", "programminghumor", "aww",
            "programmingpuns", "rareinsults", "shittyprogramming", "shittyrobots", "softwaregore", "programmingmemes",
            "whitepeopletwitter", "blackpeopletwitter", "whitepeoplegifs", "idiotsincars", "natureisfuckinglit", "dankmemes",
            "itookapicture", "catsinsinks", "animalsbeingderps", "acab", "badfaketexts", "abandonedporn", "chihuahua", "chemicalreactiongifs",
            "shittyfoodporn", "animalsbeingjerks", "animalsbeingbros", "wigglebutts", "humanporn", "techsupportgore", "iiiiiiitttttttttttt", "fffffffuuuuuuuuuuuu"};

        public Reddit(ILogger<Reddit> logger, 
            IConfiguration configuration,
            ISubredditRepository subredditRepository,
            IServerRepository serverRepository,
            IServerService servers)
        {
            _logger = logger;
            _configuration = configuration;
            _subredditRepository = subredditRepository;
            _serverRepository = serverRepository;
            _servers = servers;
        }

        [Command("reddit", RunMode = RunMode.Async)]
        [Summary("Show a reddit post")]
        public async Task RedditPost([Summary("The subreddit from which to show a post")]string subreddit = null)
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

            if(!await AddSubRedditIfNotKnownAndLearningEnabled(subreddits, subreddit))
            {
                return;
            }

            HttpClient httpClient = new HttpClient();
            string httpResult = string.Empty;
            try
            {
                httpResult = await httpClient.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Exception thrown downloading reddit post!");
                await ReplyAsync($"HttpClient encountered an error: {ex.StatusCode}");
            }

            //bool showNSFW = channel.IsNsfw;
            //if(httpResult.ToLowerInvariant().Contains("nsfw") && showNSFW != true)
            //{
            //    await ReplyAsync("NSFW Posts only shown on NSFW channels");
            //    return;
            //}

            await RemoveSubredditIfNonexistant(httpResult, subreddit);

            JArray arr = JArray.Parse(httpResult);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

            if(post["over_18"].ToString() == "True" && !channel.IsNsfw)
            {
                await ReplyAsync("NSFW Posts only shown on NSFW channels");
                return;
            }

            await CreateAndSendEmbed(post, subreddit);
        }

        [Command("subredditremove", RunMode = RunMode.Async)]
        [RequireRole("redditor")]
        [Summary("Remove a subreddit")]
        public async Task RemoveSubreddit([Summary("The subreddit to remove")]string subredditParam)
        {
            await Context.Channel.TriggerTypingAsync();
            _logger.LogInformation("{username}#{discriminator} invoked removesubreddit with subreddit {subreddit}", 
                Context.User.Username, Context.User.Discriminator, subredditParam);

            var subreddit = await _subredditRepository.GetSubredditByServerId(Context.Guild.Id, subredditParam);
            if(subreddit == null)
            {
                await ReplyAsync($"{subredditParam} was not known.");
                return;
            }


            await _subredditRepository.DeleteAsync(Context.Guild.Id, subreddit.Id);
            await ReplyAsync($"Removed {subredditParam}");
        }

        [Command("subredditlearning", RunMode = RunMode.Async)]
        [Summary("Enable or disable learning")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SubredditLearning(string value = null)
        {
            var server = await _serverRepository.GetByServerId(Context.Guild.Id);
            if (server == null)
            {
                await _serverRepository.AddAsync(Context.Guild.Id);
                server = await _serverRepository.GetByServerId(Context.Guild.Id);
            }

            if(value == null)
            {
                await ReplyAsync($"Subreddit learning is {(server.SubredditLearning ? "enabled" : "disabled")}");
                return;
            }

            if (value.ToLowerInvariant() == "on")
            {
                server.SubredditLearning = true;
                //await ReplyAsync("Subreddit learning enabled");
                await Context.Channel.SendEmbedAsync("Subreddit Learning", "Subreddit learning enabled", await _servers.GetEmbedColor(Context.Guild.Id));
                await _servers.SendLogsAsync(Context.Guild, "Subreddit Learning", $"{Context.User.Mention} enabled subreddit learning");
            }
            else if (value.ToLowerInvariant() == "off")
            {
                server.SubredditLearning = false;
                //await ReplyAsync("Subreddit learning disabled");
                await Context.Channel.SendEmbedAsync("Subreddit Learning", "Subreddit learning disabled", await _servers.GetEmbedColor(Context.Guild.Id));
                await _servers.SendLogsAsync(Context.Guild, "Subreddit Learning", $"{Context.User.Mention} disabled subreddit learning");
            }
            else
            {
                await ReplyAsync("Valid options are `on` or `off`");
                return;
            }

            await _serverRepository.EditAsync(server);
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
                .WithColor(await _servers.GetEmbedColor(Context.Guild.Id))
                .WithTitle(postTitle.ToString())
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter($"🗨 {post["num_comments"]} ⬆️ {post["ups"]}")
                .WithCurrentTimestamp();

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        private async Task RemoveSubredditIfNonexistant(string httpResult, string subreddit)
        {
            // TODO CHECK THIS
            if (!httpResult.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync($"{subreddit} does not exist!");

                await _subredditRepository.DeleteAsync(subreddit.ToLowerInvariant());
                _logger.LogDebug("reddit: Removed {subreddit}", subreddit);

                return;
            }
        }

        private async Task<bool> AddSubRedditIfNotKnownAndLearningEnabled(List<Subreddit> subreddits, string subreddit)
        {
            if (!subreddits.Any(x => x.Name.ToLowerInvariant() == subreddit.ToLowerInvariant()))
            {
                var server = await _serverRepository.GetByServerId(Context.Guild.Id);
                if (!server.SubredditLearning)
                {
                    await Context.Channel.SendMessageAsync("Subreddit is not known and learning is disabled for this server.");
                    return false;
                }
                // TODO Make the role customizable
                if(!RoleHelper.CheckForRole(Context.User as SocketGuildUser, "redditor"))
                {
                    await Context.Channel.SendMessageAsync("You must have the redditor role to add subreddits.");
                    return false;
                }

                await _subredditRepository.AddAsync(Context.Guild.Id, subreddit.ToLowerInvariant());

                _logger.LogInformation("reddit: learned {subreddit}", subreddit);
                Console.WriteLine("reddit: Learned" + subreddit);

                await ReplyAsync($"I learned a new subreddit! Now I know of {subreddits.Count + 1} subreddits!");
            }

            return true;
        }

        private async Task<List<Subreddit>> GetSubreddits( )
        {
            var subreddits = await _subredditRepository.GetSubredditListByServerId(Context.Guild.Id);

            if(subreddits.Count == 0)
            {
                foreach(string seed in _seedSubreddits)
                {
                    subreddits.Add(await _subredditRepository.AddAsync(Context.Guild.Id, seed.ToLowerInvariant()));
                }
            }

            return subreddits;
        }
    }
}
