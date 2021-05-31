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
using DiscordBot.DataAccess;
using DiscordBot.Helpers;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Victoria;

namespace DiscordBot.Commands
{
    [Name("OwnerHidden")]
    public class Owner : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly ISettings _settings;
        private readonly ILogger<Owner> _logger;
        private readonly IDiscordBotSettingsRepository _discordBotSettingsRepository;
        private readonly LavaNode _lavaNode;
        private readonly IServerService _servers;
        private readonly Random _random = new Random();

        //TODO store these in the database
        private readonly List<string> _quitImages = new List<string>
        {
            "https://i.makeagif.com/media/11-18-2014/2oMnrI.gif", "https://news.efinancialcareers.com/binaries/content/gallery/efinancial-careers/articles/2017/11/I-quit_twinsterphoto_GettyImages.jpg",
            "https://myzol.co.zw/Data/Articles/2387/bye-quit__zoom.png", "https://c1.staticflickr.com/3/2199/3527660157_2827558f95_z.jpg", "https://cdn.tinybuddha.com/wp-content/uploads/2015/08/Bad-Day.png",
            "https://geekologie.com/2015/08/03/dead-hitchhiking-robot.jpg"
        };

        public Owner(DiscordSocketClient client,
            ISettings settings,
            ILogger<Owner> logger,
            IDiscordBotSettingsRepository discordBotSettingsRepository,
            LavaNode lavaNode,
            IServerService servers)
        {
            _client = client;
            _settings = settings;
            _logger = logger;
            _discordBotSettingsRepository = discordBotSettingsRepository;
            _lavaNode = lavaNode;
            _servers = servers;
        }

        [Command("lavalink")]
        [RequireOwner]
        [Summary("Start or stop lavalink")]
        public async Task LavaLink(string enable = null)
        {
            if(enable == null)
            {
                await ReplyAsync("LavaLink is " + (LavaLinkHelper.isLavaLinkRunning() ? "" : "not" ) + " running.");
                return;
            }

            if (enable.ToLowerInvariant() == "start")
            {
                LavaLinkHelper.StartLavaLink();
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

            await ReplyAsync("Done!");
        }

        [Command("quit")]
        [RequireOwner]
        [Alias("stop")]
        [Summary("Make the bot quit!")]
        public async Task Quit(string imageUrl = null)
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
                if (_settings.EnableLavaLink)
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

                var memoryStream = await ImageHelper.FetchImage(imageUrl ?? _quitImages[_random.Next(_quitImages.Count)]);
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
        public async Task SetGame([Remainder]string game)
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
                                    .WithDescription($"DiscordBot Stopped by {Context.User.Username}\nMIT License Copyright(c) 2021 JoyfulReaper\nhttps://github.com/JoyfulReaper/DiscordBot")
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
