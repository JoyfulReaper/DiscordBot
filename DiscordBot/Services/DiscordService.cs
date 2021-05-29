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
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    class DiscordService : InitializedService
    {
        public static bool ShowJoinAndPartMessages { get; set; }

        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly CommandService _commands;
        private readonly ILogger _logger;
        private readonly IDiscordBotSettingsRepository _discordBotSettingsRepository;
        private readonly IServerService _servers;

        public DiscordService(IServiceProvider serviceProvider,
            DiscordSocketClient client,
            IConfiguration configuration,
            CommandService commands,
            ILogger<DiscordService> logger,
            IDiscordBotSettingsRepository discordBotSettingsRepository,
            IServerService servers)
        {
            _serviceProvider = serviceProvider;
            _client = client;
            _configuration = configuration;
            _commands = commands;
            _logger = logger;
            _discordBotSettingsRepository = discordBotSettingsRepository;
            _servers = servers;
        }

        private Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage message))
            {
                _logger.LogWarning("socketMessage not is not a SocketUserMessage");
                return Task.CompletedTask;
            }

            var guildName = (message.Channel as SocketGuildChannel)?.Name;

            _logger.LogInformation("Message was received from {user} on: {server}/{channel}: {message}",
                 message.Author.Username, guildName ?? "DM", message.Channel, message.Content);

            return Task.CompletedTask;
        }

        private Task OnDisconncted(Exception arg)
        {
            _logger.LogError(arg, "SocketClient disconnected!");
            Console.WriteLine("SocketClient disconnected!");

            return Task.CompletedTask;
        }

        private async Task OnReady()
        {
            _logger.LogInformation("SocketClient is Ready!");
            _logger.LogInformation("Connected as {username}#{discriminator}", _client.CurrentUser.Username, _client.CurrentUser.Discriminator);

            try
            {
                ShowJoinAndPartMessages = bool.Parse(_configuration.GetSection("ShowBotJoinMessages").Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse ShowBotJoinMessages. Using false.");
                ShowJoinAndPartMessages = false;
            }

            var settings = await _discordBotSettingsRepository.Get();
            await _client.SetGameAsync(settings.Game);

            if (ShowJoinAndPartMessages)
            {
                foreach (var guild in _client.Guilds)
                {
                    foreach (var channel in guild.Channels)
                    {
                        if (channel.Name.ToLowerInvariant() == "bot" || channel.Name.ToLowerInvariant().Contains("bot-spam"))
                        {
                            if (channel != null && channel is SocketTextChannel textChannel)
                            {
                                var builder = new EmbedBuilder()
                                    .WithThumbnailUrl(_client.CurrentUser.GetAvatarUrl() ?? _client.CurrentUser.GetDefaultAvatarUrl())
                                    .WithDescription("DiscordBot Starting\nMIT License Copyright(c) 2021 JoyfulReaper\nhttps://github.com/JoyfulReaper/DiscordBot")
                                    .WithColor(await _servers.GetEmbedColor(guild.Id))
                                    .WithCurrentTimestamp();

                                var embed = builder.Build();
                                await textChannel.SendMessageAsync(null, false, embed);
                            }
                        }
                    }
                }
            }
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            _client.Ready += OnReady;
            _client.MessageReceived += OnMessageReceived;
            _client.Disconnected += OnDisconncted;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }
    }
}
