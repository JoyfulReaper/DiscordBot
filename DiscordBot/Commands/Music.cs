using Discord;
using Discord.Commands;
using DiscordBot.Helpers;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace DiscordBot.Commands
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;
        private readonly ILogger<Music> _logger;
        private readonly ISettings _settings;

        public Music(LavaNode lavaNode,
            ILogger<Music> logger,
            ISettings settings)
        {
            _lavaNode = lavaNode;
            _logger = logger;
            _settings = settings;
        }

        [Command("search")]
        [Summary("search youtube")]
        public async Task Search([Remainder]string query)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed search ({query}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, query, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if(!CheckIfLavaLinkIsEnabled())
            {
                return;
            }

            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach(var track in searchResponse.Tracks)
            {
                sb.AppendLine($"{track.Author} - {track.Title} ({track.Duration})");
            }

            await ReplyAsync(sb.ToString());
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Play a song from YouTube")]
        public async Task PlayAsync([Remainder] string query)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed play ({query}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, query, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if (!CheckIfLavaLinkIsEnabled())
            {
                return;
            }

            if (await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                await ReplyAsync("Joining you ;-)");
                await JoinAsync();
            }

            //var searchResponse = await _lavaNode.SearchAsync(query);
            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{query}`.");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
                {
                    foreach (var track in searchResponse.Tracks)
                    {
                        player.Queue.Enqueue(track);
                    }

                    await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    var track = searchResponse.Tracks[0];
                    player.Queue.Enqueue(track);
                    await ReplyAsync($"Enqueued: {track.Title}");
                }
            }
            else
            {
                var track = searchResponse.Tracks[0]; // First result, maybe a random one?

                if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name)) // Play a playlist, maybe make this an option?
                {
                    for (var i = 0; i < searchResponse.Tracks.Count; i++)
                    {
                        if (i == 0)
                        {
                            await player.PlayAsync(track);
                            await ReplyAsync($"Now Playing: {track.Title}");
                        }
                        else
                        {
                            player.Queue.Enqueue(searchResponse.Tracks[i]);
                        }
                    }

                    await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} tracks.");
                }
                else
                {
                    await player.PlayAsync(track);
                    await ReplyAsync($"Now Playing: {track.Title}");
                }
            }
        }

        [Command("Join", RunMode = RunMode.Async)]
        [Summary("Join voice channel")]
        public async Task JoinAsync()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed join on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if (!CheckIfLavaLinkIsEnabled())
            {
                return;
            }

            if (await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if(!await CheckIfUserIsInVoiceChannel(voiceState))
            {
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("skip", RunMode = RunMode.Async)]
        [Summary("Skip the currently playing track")]
        public async Task Skip()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed skip on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if (!CheckIfLavaLinkIsEnabled())
            {
                return;
            }

            if (await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (!await CheckForValidState(voiceState))
            {
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.Queue.Count == 0)
            {
                await ReplyAsync("There are no more songs in the queue!");
                return;
            }

            await player.SkipAsync();
            await ReplyAsync($"Skipped! Now playing **{player.Track.Title}**");
        }

        [Command("pause", RunMode = RunMode.Async)]
        [Summary("Pause the currently playing track")]
        public async Task Pause()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed pause on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if (!CheckIfLavaLinkIsEnabled())
            {
                return;
            }

            if (await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (!await CheckForValidState(voiceState))
            {
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.PlayerState == PlayerState.Paused || player.PlayerState == PlayerState.Stopped)
            {
                await ReplyAsync("No music is playing.");
                return;
            }

            await player.PauseAsync();
            await ReplyAsync("Music is paused!");
        }

        [Command("resume", RunMode = RunMode.Async)]
        [Summary("Resume track")]
        public async Task Resume()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed resume on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if (!CheckIfLavaLinkIsEnabled())
            {
                return;
            }

            if (await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (!await CheckForValidState(voiceState))
            {
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (player.PlayerState == PlayerState.Playing)
            {
                await ReplyAsync("Music is already playing");
                return;
            }

            await player.ResumeAsync();
            await ReplyAsync("Music has resumed!");
        }

        private async Task<bool> CheckForValidState(IVoiceState voiceState)
        {
            if (!await CheckIfUserIsInVoiceChannel(voiceState))
            {
                return false;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return false;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("You need to be in the same voice channel as me!");
                return false;
            }

            return true;
        }

        private async Task<bool> CheckIfUserIsInVoiceChannel(IVoiceState voiceState)
        {
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return false;
            }

            return true;
        }

        private bool CheckIfLavaLinkIsEnabled()
        {
            if(!LavaLinkHelper.isLavaLinkRunning())
            {
                ReplyAsync("Lavalink is not running :(");
                return false;
            }

            return true;
        }
    }
}
