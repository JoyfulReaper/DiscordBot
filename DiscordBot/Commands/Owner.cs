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
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Owner : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly Settings _settings;
        private readonly ILogger<Owner> _logger;
        private readonly IDiscordBotSettingsRepository _discordBotSettingsRepository;

        public Owner(DiscordSocketClient client,
            Settings settings,
            ILogger<Owner> logger,
            IDiscordBotSettingsRepository discordBotSettingsRepository)
        {
            _client = client;
            _settings = settings;
            _logger = logger;
            _discordBotSettingsRepository = discordBotSettingsRepository;
        }

        [Command("quit")]
        [Alias("stop")]
        [Summary("Make the bot quit!")]
        public async Task Quit()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} invoked quit on {target}", Context.User.Username, Context.User.Discriminator, Context.Guild.Name);

            if (Context.User.Username != _settings.OwnerName || Context.User.Discriminator != _settings.OwnerDiscriminator)
            {
                await ReplyAsync("Sorry, only the bot's owner can make the bot quit!");
            }
            else
            {
                await ReplyAsync("Please, no! I want to live! Noooo.....");
                var message = await ReplyAsync("https://i.makeagif.com/media/11-18-2014/2oMnrI.gif");
                await Task.Delay(2500);
                await message.DeleteAsync();

                foreach (var guild in _client.Guilds)
                {
                    foreach (var channel in guild.Channels)
                    {
                        if (channel.Name.ToLowerInvariant() == "bot" || channel.Name.ToLowerInvariant().StartsWith("bot-spam"))
                        {
                            if (channel != null && channel is SocketTextChannel textChannel && DiscordService.ShowJoinAndPartMessages)
                            {
                                var builder = new EmbedBuilder()
                                    .WithThumbnailUrl(_client.CurrentUser.GetAvatarUrl())
                                    .WithDescription($"DiscordBot Stopped by {Context.User.Username}\nMIT License Copyright(c) 2021 JoyfulReaper\nhttps://github.com/JoyfulReaper/DiscordBot")
                                    .WithColor(ColorHelper.GetColor())
                                    .WithCurrentTimestamp();

                                var embed = builder.Build();
                                await textChannel.SendMessageAsync(null, false, embed);
                            }
                        }
                    }
                }

                await _client.StopAsync(); // Allow the client to cleanup
            }
        }

        [Command("game")]
        [Summary("Set the same the bot is playing")]
        public async Task SetGame([Remainder]string game)
        {
            _logger.LogInformation("{username}#{discriminator} invoked game of {game}", Context.User.Username, Context.User.Discriminator, game);

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
            }
        }
    }
}
