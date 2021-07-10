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
using DiscordBotLib.DataAccess;
using DiscordBotLib.Helpers;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Victoria;

namespace DiscordBot.Commands
{
    [Name("OwnerHidden")]
    public class OwnerModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly ISettings _settings;
        private readonly ILogger<OwnerModule> _logger;
        private readonly IDiscordBotSettingsRepository _discordBotSettingsRepository;
        private readonly LavaNode _lavaNode;
        private readonly IServerService _servers;
        private readonly IServerRepository _serverRepository;

        public OwnerModule(DiscordSocketClient client,
            ISettings settings,
            ILogger<OwnerModule> logger,
            IDiscordBotSettingsRepository discordBotSettingsRepository,
            LavaNode lavaNode,
            IServerService servers,
            IServerRepository serverRepository)
        {
            _client = client;
            _settings = settings;
            _logger = logger;
            _discordBotSettingsRepository = discordBotSettingsRepository;
            _lavaNode = lavaNode;
            _servers = servers;
            _serverRepository = serverRepository;
        }

        [Command("broadcast")]
        [RequireOwner]
        [Summary("Send a message to the logging channel of ever server the bot is in (if set)")]
        public async Task BroadCast([Summary("Broadcast message")][Remainder]string message)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed broadcast ({message}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, message, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var sent = 0;
            foreach (var guild in _client.Guilds)
            {
                //var server = await _servers.GetServer(guild);
                var server = await ServerHelper.GetOrAddServer(guild.Id, _serverRepository);
                if(server.LoggingChannel != 0)
                {
                    // Logging channel set
                    var channel = _client.GetChannel(server.LoggingChannel) as ISocketMessageChannel;
                    if (channel != null)
                    {
                        await channel.SendEmbedAsync("Owner Broadcast Message", $"Message: {message}", ColorHelper.GetColor(server), ImageLookupUtility.GetImageUrl("BROADCAST_IMAGES"));
                    }
                    sent++;
                }
                else
                {
                    // Logging channel not set
                    var owner = guild.Owner as SocketUser;
                    var ownerChannel = await owner?.GetOrCreateDMChannelAsync();
                    if (ownerChannel != null)
                    {
                        //await ownerChannel.SendEmbedAsync("Owner Broadcast Message", $"Message: {message}\n" +
                        //    $"Set the logging channel with: {server.Prefix}logs channel {{channelMention}} to avoid DMs from the bot!", ColorHelper.GetColor(server), ImageLookupUtility.GetImageUrl("BROADCAST_IMAGES"));
                        EmbedBuilder builder = new EmbedBuilder();
                        builder.Title = "Owner Broadcast Message";
                        builder.Description = $"Message: { message}\n" +
                            $"Set the logging channel with: {server.Prefix}logs channel {{channelMention}} to avoid DMs from the bot!";
                        builder.Color = ColorHelper.GetColor(server);
                        builder.ThumbnailUrl = ImageLookupUtility.GetImageUrl("BROADCAST_IMAGES");
                        builder.WithCurrentTimestamp();

                        await ownerChannel.SendMessageAsync(null, false, builder.Build());
                    }
                }
            }

            await _servers.SendLogsAsync(Context.Guild, "Broadcast Message Sent", $"{Context.User.Mention} sent a broadcast message: {message}", ImageLookupUtility.GetImageUrl("LOGGING_IMAGES"));
            await ReplyAsync($"Broadcast message has been sent to {sent} server out of {_client.Guilds.Count} ({_client.Guilds.Count - sent} do not have logging channel set!)");
        }

        [Command("lavalink")]
        [RequireOwner]
        [Summary("Start or stop lavalink")]
        public async Task LavaLink([Summary("start or stop lavalink")]string enable = null)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed lavalink ({option}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, enable, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if (enable == null)
            {
                await ReplyAsync("LavaLink is " + (LavaLinkHelper.isLavaLinkRunning() ? "" : "not" ) + " running.");
                return;
            }

            if (enable.ToLowerInvariant() == "start")
            {
                LavaLinkHelper.StartLavaLink();
                await Task.Delay(5000);

                if(!_lavaNode.IsConnected)
                {
                    await _lavaNode.ConnectAsync();
                }
            }
            else if (enable.ToLowerInvariant() == "stop")
            {
                if (_lavaNode.IsConnected)
                {
                    await _lavaNode.DisconnectAsync();
                }

                LavaLinkHelper.StopLavaLink();
            }
            else
            {
                await ReplyAsync("Would you like to `start` or `stop` lavalink?");
                return;
            }

            await Context.Channel.SendEmbedAsync("Lava Link", $"Lavalink was {(enable.ToLowerInvariant() == "start" ? "started" : "stopped")}!",
                ColorHelper.GetColor(await _servers.GetServer(Context.Guild)));

            await _servers.SendLogsAsync(Context.Guild, "Lavalink", $"Lavalink was {(enable.ToLowerInvariant() == "start" ? "started": "stopped")} by {Context.User.Mention}!");
        }

        [Command("quit")]
        [RequireOwner]
        [Alias("stop")]
        [Summary("Make the bot quit!")]
        public async Task Quit([Summary("The image to display when the bot quits")]string imageUrl = null)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} invoked quit on {server}/{channel}", 
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if (Context.User.Username != _settings.OwnerName || Context.User.Discriminator != _settings.OwnerDiscriminator)
            {
                await ReplyAsync("Sorry, only the bot's owner can make the bot quit!");
                return;
            }
            else
            {
                if (LavaLinkHelper.isLavaLinkRunning())
                {
                    await _lavaNode.DisconnectAsync();
                }

                IUserMessage message;
                await ReplyAsync("Please, no! I want to live! Noooo.....");

                if(imageUrl != null)
                {
                    // Delete the quit command so the image isn't shown twice
                    await Context.Message.DeleteAsync();
                }

                var memoryStream = await ImageHelper.FetchImage(imageUrl ?? ImageLookupUtility.GetImageUrl("QUIT_IMAGES"));
                if(memoryStream == null)
                {
                    await ReplyAsync("Quit Image could not be fetched! Bye anyway!");
                    ShowQuitMessageIfEnabled();
                    Program.ExitCleanly();
                }
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                message = await Context.Channel.SendFileAsync(memoryStream, "quitimage.png");

                await Task.Delay(3000);
                await message.DeleteAsync();

                ShowQuitMessageIfEnabled();
                await _servers.SendLogsAsync(Context.Guild, "Bot quitting", $"{Context.User.Mention} has requested the bot to terminate.");

                await _client.StopAsync(); // Allow the client to cleanup
                Program.ExitCleanly();
            }
        }

        [Command("game")]
        [RequireOwner]
        [Summary("Set the same the bot is playing")]
        public async Task SetGame([Summary("The game for the bot to play")][Remainder]string game)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} invoked game ({game}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, game, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if (Context.User.Username != _settings.OwnerName || Context.User.Discriminator != _settings.OwnerDiscriminator)
            {
                await ReplyAsync("Sorry, only the bot's owner can set the game!");
            }
            else
            {
                var settings = await _discordBotSettingsRepository.Get();
                await _client.SetGameAsync(game);
                settings.Game = game;
                await _discordBotSettingsRepository.EditAsync(settings);

                await ReplyAsync("Game changed!");
                await _servers.SendLogsAsync(Context.Guild, "Game Updated", $"{Context.User.Mention} has changed the game to {game}.");
            }
        }

        private async void ShowQuitMessageIfEnabled()
        {
            if (DiscordService.ShowJoinAndPartMessages)
            {
                foreach (var guild in _client.Guilds)
                {
                    foreach (var channel in guild.Channels)
                    {
                        if (channel.Name.ToLowerInvariant() == "bot" || channel.Name.ToLowerInvariant().StartsWith("bot-spam"))
                        {
                            if (channel != null && channel is SocketTextChannel textChannel)
                            {
                                var builder = new EmbedBuilder()
                                    .WithThumbnailUrl(_client.CurrentUser.GetAvatarUrl())
                                    .WithDescription($"{_settings.BotName} stopped by {Context.User.Username}\nMIT License Copyright(c) 2021 JoyfulReaper\n{_settings.BotWebsite}")
                                    .WithColor(await _servers.GetEmbedColor(Context.Guild.Id))
                                    .WithCurrentTimestamp();

                                var embed = builder.Build();
                                await textChannel.SendMessageAsync(null, false, embed);
                            }
                        }
                    }
                }
            }
        }
    }
}
