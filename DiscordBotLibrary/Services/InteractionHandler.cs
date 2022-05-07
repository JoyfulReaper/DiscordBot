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
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System.Reflection;


namespace DiscordBotLibrary.Services;

public class InteractionHandler : IInteractionHandler, IDisposable
{
    private readonly InteractionService _interactionService;
    private readonly ILoggingService _loggingService;
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;
    private readonly ILogger<InteractionService> _logger;

    public InteractionHandler(InteractionService interactionService,
        ILoggingService loggingService,
        IServiceProvider services,
        DiscordSocketClient client,
        ILogger<InteractionService> logger)
    {
        _interactionService = interactionService;
        _loggingService = loggingService;
        _services = services;
        _client = client;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _client.InteractionCreated += HandleInteractionAsync;
        //_client.UserCommandExecuted += UserCommandExcuted;
        _interactionService.SlashCommandExecuted += SlashCommandExcuted;
        //_interactionService.ContextCommandExecuted += ContextCommandExcuted;
        //_interactionService.ComponentCommandExecuted += ComponentCommentExecuted;
        //_interactionService.InteractionExecuted += InteractionExcuted;
        _interactionService.Log += _loggingService.LogAsync;
        

        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }
    
    private async Task HandleInteractionAsync(SocketInteraction interactionParam)
    {
        try
        {
            var ctx = new SocketInteractionContext(_client, interactionParam);
            await _interactionService.ExecuteCommandAsync(ctx, _services);
        }
        catch (Exception ex)
        {
            await _loggingService.LogAsync(new LogMessage(Discord.LogSeverity.Warning, "InteractionHandler", $"Error handling interaction: {ex.Message}", ex));

            if (interactionParam.Type == InteractionType.ApplicationCommand)
            {
                await interactionParam.GetOriginalResponseAsync()
                    .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }
    }

    private async Task SlashCommandExcuted(SlashCommandInfo slashInfo,
        IInteractionContext context,
        IResult result)
    {
        if(!result.IsSuccess)
        {
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                    // implement
                    break;
                case InteractionCommandError.UnknownCommand:
                    _logger.LogInformation("{user}#{discriminator} attempted to use an unknown slash command: {slashCommand} on {guild}/{channel}",
                        context.User.Username, context.User.Discriminator, slashInfo?.Name ?? "(unknown slash command)", context.Guild?.Name ?? "DM", context.Channel.Name);

                    _ = Task.Run(async () =>
                    {
                        var badCommandMessage = await context.Channel.SendMessageAsync(ImageLookup.GetImageUrl("BADCOMMAND_IMAGES"));
                        await Task.Delay(3500);
                        await badCommandMessage.DeleteAsync();
                    });

                    return;
                case InteractionCommandError.BadArgs:
                    // implement
                    break;
                case InteractionCommandError.Exception:
                    _logger.LogInformation(((ExecuteResult)result).Exception, "An exception occured in {slashCommand} for {user}#{discriminator}  on {guild}/{channel}",
                        slashInfo?.Name ?? "(unknown slash command)", context.User.Username, context.User.Discriminator, context.Guild?.Name ?? "DM", context.Channel.Name);

                    await context.Channel.SendMessageAsync($"The slash command threw an exception :(\nError: {result.ErrorReason}");
                    return;
                case InteractionCommandError.Unsuccessful:
                    _logger.LogDebug("Slash command {slashcommand} was unsuccessful for {user}#{discriminator} on {guild}/{channel}: {error}",
                        slashInfo?.Name ?? "(unknown slash command)", context.User.Username, context.User.Discriminator, context.Guild?.Name ?? "DM", context.Channel.Name, result.ErrorReason);

                    await context.Channel.SendMessageAsync($"The slash command {slashInfo?.Name ?? "(unknown slash command)"} was unsuccessful :(\nError: {result.ErrorReason}");
                    return;
                default:
                    break;
            }

            _logger.LogInformation("{user}#{discriminator} failed to execute an {interaction} on {guild}/{channel}: {reason}",
                context.User.Username, context.User.Discriminator, slashInfo.Name, context.Guild?.Name ?? "DM", context.Channel.Name, result.ErrorReason);

            await context.Channel.SendMessageAsync($"Something went wrong :(\nError: {result.ErrorReason}");
        }
    }

    public void Dispose()
    {
        _client.InteractionCreated -= HandleInteractionAsync;
        //_client.UserCommandExecuted += UserCommandExcuted;
        _interactionService.SlashCommandExecuted -= SlashCommandExcuted;
        //_interactionService.ContextCommandExecuted += ContextCommandExcuted;
        //_interactionService.ComponentCommandExecuted += ComponentCommentExecuted;
        //_interactionService.InteractionExecuted += InteractionExcuted;
        _interactionService.Log -= _loggingService.LogAsync;
    }
}
