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
using Discord.WebSocket;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DiscordBotLibrary.Services;

public class DiscordService : IDiscordService
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly ICommandHandler _commandHandler;
    private readonly IInteractionHandler _interactionHandler;
    private readonly ILoggingService _loggingService;

    public DiscordService(DiscordSocketClient discordSocketClient,
        IConfiguration config,
        ICommandHandler commandHandler,
        IInteractionHandler interactionHandler,
        ILoggingService loggingService)
    {
        _client = discordSocketClient;
        _config = config;
        _commandHandler = commandHandler;
        _interactionHandler = interactionHandler;
        _loggingService = loggingService;

        _client.Log += loggingService.LogAsync;
    }

    public async Task Start()
    {
        await _commandHandler.Initialize();
        await _interactionHandler.Initialize();
        
        // TODO: Store the token in the database
        await _client.LoginAsync(TokenType.Bot, _config["Token"]);
        await _client.StartAsync();
    }

    public async Task Stop()
    {
        await _client.LogoutAsync();
        Environment.Exit(0);
    }
}
