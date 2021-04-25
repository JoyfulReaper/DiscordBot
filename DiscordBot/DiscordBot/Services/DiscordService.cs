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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class DiscordService : IChatService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly CommandService _commands;
        private readonly ILogger _logger;

        public DiscordService(IServiceProvider serviceProvider,
            DiscordSocketClient client,
            IConfiguration configuration,
            CommandService commands,
            ILogger<DiscordService> logger)
        {
            _serviceProvider = serviceProvider;
            _client = client;
            _configuration = configuration;
            _commands = commands;
            _logger = logger;

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

            Console.WriteLine("SocketClient is ready");
            Console.WriteLine($"Connected as {_client.CurrentUser.Username}#{_client.CurrentUser.Discriminator}");

            foreach(var guild in _client.Guilds)
            {
                foreach(var channel in guild.Channels)
                {
                    if(channel.Name.ToLowerInvariant() == "bot" || channel.Name.ToLowerInvariant().StartsWith("bot-spam"))
                    {
                        if(channel != null && channel is SocketTextChannel textChannel)
                        {
                            await textChannel.SendMessageAsync("Beep boop! I'm alive!");
                            await textChannel.SendMessageAsync("MIT License by JoyfulReaper: https://github.com/JoyfulReaper/DiscordBot");
                        }
                    }
                }
            }
        }

        public async Task Start()
        {
            //TODO Figure out a better place to store the token Maybe in a DB
            _logger.LogInformation("Reading token from config file");
            var token = File.ReadAllText(@"C:\token.txt");

            if(string.IsNullOrEmpty(token))
            {
                _logger.LogCritical("Token is null or empty");
                throw new InvalidOperationException("Token is null or empty");
            }

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }
    }
}
