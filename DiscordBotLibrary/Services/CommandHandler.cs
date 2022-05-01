/*
MIT License

Copyright(c) 2022 Kyle Givler
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
using DiscordBotLibrary.ConfigSections;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace DiscordBotLibrary.Services;

public class CommandHandler : ICommandHandler
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commandService;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILoggingService _loggingService;
    private readonly BotInformation _botInfo;

    public CommandHandler(DiscordSocketClient client,
        CommandService commandService,
        IServiceProvider services,
        IConfiguration config,
        ILoggingService loggingService)
    {
        _client = client;
        _commandService = commandService;
        _services = services;
        _config = config;
        _loggingService = loggingService;
        
        _botInfo = _config.GetSection("BotInformation").Get<BotInformation>();

        _commandService.Log += _loggingService.LogAsync;
    }

    public async Task Initialize()
    {
        _client.MessageReceived += HandleCommandAsync;
        _commandService.CommandExecuted += CommandExecutedAsync;

        await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(),
             _services);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        int argPos = 0;

        if (!(message.HasStringPrefix(_botInfo.DefaultPrefix, ref argPos) ||
            message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;

        var context = new SocketCommandContext(_client, message);

        await _commandService.ExecuteAsync(
            context: context,
            argPos: argPos,
            services: _services);
    }

    private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
    {
        if (result.IsSuccess)
        {
            await _loggingService.LogAsync(new LogMessage(LogSeverity.Info, "CommandHandler",
                $"{context.User.Username}#{context.User.Discriminator} successfully executed {command.Value.Name} on {context.Guild?.Name ?? "DM"}/{context.Channel.Name}"));
        }

        if (!string.IsNullOrEmpty(result?.ErrorReason))
        {
            await _loggingService.LogAsync(new LogMessage(LogSeverity.Info, "CommandHandler",
                $"{context.User.Username}#{context.User.Discriminator} failed to executed {command.Value.Name} on {context.Guild?.Name ?? "DM"}/{context.Channel.Name}: {result.ErrorReason}"));
            
            await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
