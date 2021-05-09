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
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommandHandler> _logger;

        private readonly string _prefix;

        public CommandHandler(DiscordSocketClient client,
            CommandService commands,
            IConfiguration config,
            IServiceProvider serviceProvider,
            ILogger<CommandHandler> logger)
        {
            _client = client;
            _commands = commands;
            _config = config;
            _serviceProvider = serviceProvider;
            _logger = logger;

            _prefix = _config.GetSection("Prefix").Value;
            _logger.LogInformation("Prefix set to {prefix}", _prefix);

            _client.MessageReceived += OnMessageReceived;
            _commands.CommandExecuted += OnCommandExecuted;
        }

        private async Task OnMessageReceived(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;

            if(message == null)
            {
                _logger.LogDebug("message was null");
                return;
            }

            var context = new SocketCommandContext(_client, message);
            int position = 0;

            if(message.HasStringPrefix(_prefix, ref position) || message.HasMentionPrefix(_client.CurrentUser, ref position))
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
                //TODO Message about this handler blocking the gateway thread, wrapping in a Task.Run didn't fix
                // Look into this more
                await Task.Run(async () =>
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

                _logger.LogError("Error Occured for command {command}: {error}", context.Message.Content, result.Error);
                Console.WriteLine($"The following error occured: {result.Error}");
            }
        }
    }
}
