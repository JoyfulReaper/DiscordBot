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
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly Settings _settings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommandHandler> _logger;
        private readonly IServerService _servers;
        private readonly ImageService _images;
        private readonly IConfiguration _configuration;
        private readonly IAutoRoleService _autoRoleService;

        public CommandHandler(DiscordSocketClient client,
            CommandService commands,
            Settings settings,
            IServiceProvider serviceProvider,
            ILogger<CommandHandler> logger,
            IServerService servers,
            ImageService images,
            IConfiguration configuration,
            IAutoRoleService autoRoleService)
        {
            _client = client;
            _commands = commands;
            _settings = settings;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _servers = servers;
            _images = images;
            _configuration = configuration;
            _autoRoleService = autoRoleService;

            _client.MessageReceived += OnMessageReceived;
            _client.UserJoined += OnUserJoined;

            _commands.CommandExecuted += OnCommandExecuted;
        }

        private async Task OnUserJoined(SocketGuildUser userJoining)
        {
            await AssignAutoRoles(userJoining);
            await ShowWelcomeMessage(userJoining);
        }

        private async Task AssignAutoRoles(SocketGuildUser userJoining)
        {
            var roles = await _autoRoleService.GetAutoRoles(userJoining.Guild);
            if (roles.Count < 1)
            {
                return;
            }

            await userJoining.AddRolesAsync(roles);
        }

        private async Task ShowWelcomeMessage(SocketGuildUser userJoining)
        {
            bool showMessage = false;

            try
            {
                showMessage = bool.Parse(_configuration.GetSection("ShowWelcomeMessage").Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse ShowWelecomMessage, using {value}", showMessage);
            }

            if (showMessage)
            {
                await userJoining.Guild.DefaultChannel.SendMessageAsync($"{userJoining.Username} {_settings.WelcomeMessage}");

                var channel = userJoining.Guild.DefaultChannel as ISocketMessageChannel;
                var memoryStream = await _images.CreateImage(userJoining);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                await channel.SendFileAsync(memoryStream, $"{userJoining.Username}.png");
            }
        }

        private async Task OnMessageReceived(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            if(message == null)
            {
                _logger.LogDebug("message was null");
                return;
            }

            var prefix = await _servers.GetGuildPrefix((message.Channel as SocketGuildChannel).Guild.Id);

            var context = new SocketCommandContext(_client, message);
            int position = 0;

            if(message.HasStringPrefix(prefix, ref position) 
                || message.HasMentionPrefix(_client.CurrentUser, ref position))
            {
                _logger.LogInformation("Command received: {command}", message.Content);

                if (message.Author.IsBot)
                {
                    _logger.LogDebug("Command ({command}) was sent by another bot, ignoring", message.Content);
                    return;
                }

               await _commands.ExecuteAsync(context, position, _serviceProvider);
            }
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (result.Error == CommandError.UnknownCommand)
            {
                //TODO add multiple images
                //TODO make this optional/a setting
                Task.Run(async () =>
                {
                    _logger.LogDebug("{user} attempted to use an unknown command {command}", context.User.Username, context.Message.Content);
                    var badCommandMessage = await context.Channel.SendMessageAsync("https://www.wheninmanila.com/wp-content/uploads/2017/12/meme-kid-confused.png");
                    await Task.Delay(3500);
                    await badCommandMessage.DeleteAsync();
                });

                return;
            }

            if (!result.IsSuccess)
            {
                if(result.Error == CommandError.ObjectNotFound)
                {
                    await context.Channel.SendMessageAsync($"Unknown object");
                }

                if(result.Error == CommandError.UnmetPrecondition)
                {
                    // TODO better error message
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }

                _logger.LogError("Error Occured for command {command}: {error}", context.Message.Content, result.Error);
                Console.WriteLine($"The following error occured: {result.Error}");
            }
        }
    }
}
