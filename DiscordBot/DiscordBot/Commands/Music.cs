using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace DiscordBot.Commands
{
    public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Music(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }

        [Command("play", RunMode = RunMode.Async)]
        [Summary("Play a song from YouTube")]
        public async Task PlayAsync([Remainder] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm not connected to a voice channel.");
                return;
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
            await ReplyAsync("Music is paused!");
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
    }
}
