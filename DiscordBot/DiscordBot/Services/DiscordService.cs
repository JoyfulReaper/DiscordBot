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
using DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    class DiscordService : IChatService
    {
        public static bool ShowJoinAndPartMessages { get; set; }

        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly CommandService _commands;
        private readonly ILogger _logger;
        private readonly IDiscordBotSettingsRepository _discordBotSettingsRepository;

        public DiscordService(IServiceProvider serviceProvider,
            DiscordSocketClient client,
            IConfiguration configuration,
            CommandService commands,
            ILogger<DiscordService> logger,
            IDiscordBotSettingsRepository discordBotSettingsRepository)
        {
            _serviceProvider = serviceProvider;
            _client = client;
            _configuration = configuration;
            _commands = commands;
            _logger = logger;
            _discordBotSettingsRepository = discordBotSettingsRepository;

            _client.Ready += SocketClient_Ready;
            _client.MessageReceived += SocketClient_MessageReceived;
            _client.Disconnected += SocketClient_Disconnected;
        }

        private Task SocketClient_MessageReceived(SocketMessage arg)
        {
            //TODO replace this with logging / Possibly keep a database of all messages received
            Console.WriteLine($"Message received: {arg.Author.Username} : {arg.Channel.Name} : {arg.Content}");
            _logger.LogInformation("Message Received: {author} : {channel} : {message}", arg.Author.Username, arg.Channel.Name, arg.Content);

            return Task.CompletedTask;
        }

        private Task SocketClient_Disconnected(Exception arg)
        {
            _logger.LogError(arg, "SocketClient disconnected!");

            Console.WriteLine("SocketClient disconnected!");
            Environment.Exit(1);

            return Task.CompletedTask;
        }

        private async Task SocketClient_Ready()
        {
            _logger.LogInformation("Connected as {username}#{discriminator}", _client.CurrentUser.Username, _client.CurrentUser.Discriminator);

            var settings = await _discordBotSettingsRepository.Get();
            await _client.SetGameAsync(settings.Game);

            Console.WriteLine("SocketClient is ready");
            Console.WriteLine($"Connected as {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}");

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
                                    .WithThumbnailUrl(_client.CurrentUser.GetAvatarUrl())
                                    .WithDescription("DiscordBot Starting\nMIT License Copyright(c) 2021 JoyfulReaper\nhttps://github.com/JoyfulReaper/DiscordBot")
                                    .WithColor(ColorHelper.GetColor())
                                    .WithCurrentTimestamp();

                                var embed = builder.Build();
                                await textChannel.SendMessageAsync(null, false, embed);
                            }
                        }
                    }
                }
            }
        }

        public async Task Start()
        {
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
            if(settings == null)
            {
                settings = GetTokenFromConsole();
            }

            try
            {
                await _client.LoginAsync(TokenType.Bot, settings.Token);
            }
            catch (Discord.Net.HttpException ex)
            {
                if(ex.Reason == "401: Unauthorized")
                {
                    Console.WriteLine("\nToken is incorrect.");
                    Console.Write("Enter Token: ");
                    settings.Token = Console.ReadLine();

                    await _discordBotSettingsRepository.EditAsync(settings);
                    Start();
                }
            }

            await _client.StartAsync();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private DiscordBotSettings GetTokenFromConsole()
        {
            Console.WriteLine("\nToken has not yet been saved.");
            Console.Write("Please enter the bot's token: ");
            var token = Console.ReadLine();

            DiscordBotSettings settings = new DiscordBotSettings { Token = token, Game = "https://github.com/JoyfulReaper" };
            _discordBotSettingsRepository.AddAsync(settings);

            return settings;
        }
    }
}
