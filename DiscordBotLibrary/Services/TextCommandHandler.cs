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
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace DiscordBotLibrary.Services;

/// <summary>
/// Service for handling legacy text commands
/// </summary>
public class TextCommandHandler : ITextCommandHandler, IDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commandService;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _config;
    private readonly ILoggingService _loggingService;
    private readonly ILogger<TextCommandHandler> _logger;
    private readonly BotInformation _botInfo;

    public TextCommandHandler(DiscordSocketClient client,
        CommandService commandService,
        IServiceProvider services,
        IConfiguration config,
        ILoggingService loggingService,
        ILogger<TextCommandHandler> logger)
    {
        _client = client;
        _commandService = commandService;
        _services = services;
        _config = config;
        _loggingService = loggingService;
        _logger = logger;
        _botInfo = _config.GetSection("BotInformation").Get<BotInformation>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task InitializeAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        _client.UserLeft += OnUserLeft;
        _client.UserJoined += OnUserJoined;
        _commandService.Log += _loggingService.LogAsync;
        _commandService.CommandExecuted += CommandExecutedAsync;

        await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(),
             _services);
    }

    /// <summary>
    /// Handle Legacy Text Command
    /// </summary>
    /// <param name="messageParam">The message potentially containing the command</param>
    /// <returns></returns>
    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        var message = messageParam as SocketUserMessage;
        if (message == null)
        {
            _logger.LogDebug("Unable to cast message to SocketUserMessage");
            return;
        }

        int argPos = 0;

        if (!(message.HasStringPrefix(_botInfo.DefaultPrefix, ref argPos) ||
            message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
        {
            return;
        }

        if(message.Author.IsBot)
        {
            _logger.LogDebug("Command was from a bot, ignoring: {command} Bot: {bot}", 
                message.Content, $"{message.Author.Username}#{message.Author.Discriminator}");
            return;
        }

        var context = new SocketCommandContext(_client, message);

        await _commandService.ExecuteAsync(
            context: context,
            argPos: argPos,
            services: _services);
    }


    /// <summary>
    /// Hand post command execution regardless of success or failure
    /// </summary>
    /// <param name="command"></param>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, Discord.Commands.IResult result)
    {
        // TODO Log command usage to API
        if (result.IsSuccess)
        {
             _logger.LogInformation("{user}#{discriminator} successfully executed {command} on {guild}/{channel}",
                context.User.Username, context.User.Discriminator, command.Value.Name, context.Guild?.Name ?? "DM", context.Channel.Name);
        }

        if (!string.IsNullOrEmpty(result?.ErrorReason))
        {
            if(result.Error == CommandError.UnknownCommand)
            {
                _logger.LogInformation("{user}#{discriminator} attempted to use and unknown command: {command} on {guild}/{channel}",
                   context.User.Username, context.User.Discriminator, context.Message.Content, context.Guild?.Name ?? "DM", context.Channel.Name);

                _ = Task.Run(async () => 
                {
                    var badCommandMessage = await context.Channel.SendMessageAsync(ImageLookup.GetImageUrl("BADCOMMAND_IMAGES"));
                    await Task.Delay(3500);
                    await badCommandMessage.DeleteAsync();
                });

                return;
            }

            _logger.LogInformation("{user}#{discriminator} failed to execute {command} on {guild}/{channel}: {reason}",
                context.User.Username, context.User.Discriminator, command.Value.Name, context.Guild?.Name ?? "DM", context.Channel.Name, result.ErrorReason);
            
            await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }

    private Task OnUserJoined(SocketGuildUser user)
    {
        // TODO Show join message!
        // Add 
        return Task.CompletedTask;
    }

    private Task OnUserLeft(SocketGuild guild, SocketUser user)
    {
        //TODO Show part message!
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _client.MessageReceived -= HandleCommandAsync;
        _client.UserLeft -= OnUserLeft;
        _client.UserJoined -= OnUserJoined;
        _commandService.Log -= _loggingService.LogAsync;
        _commandService.CommandExecuted -= CommandExecutedAsync;
    }
}
