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
using DiscordBotLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Services;

public class InteractionHandler : IInteractionHandler
{
    private readonly InteractionService _interactionService;
    private readonly ILoggingService _loggingService;
    private readonly IServiceProvider _services;
    private readonly DiscordSocketClient _client;

    public InteractionHandler(InteractionService interactionService,
        ILoggingService loggingService,
        IServiceProvider services,
        DiscordSocketClient client)
    {
        _interactionService = interactionService;
        _loggingService = loggingService;
        _services = services;
        _client = client;
    }


    public async Task Initialize()
    {
        _client.InteractionCreated += HandleInteractionAsync;
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
}
