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
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DiscordBotLibrary.Services;

public class DiscordService : IDiscordService
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _config;
    private readonly ITextCommandHandler _commandHandler;
    private readonly IInteractionHandler _interactionHandler;
    private readonly ILoggingService _loggingService;
    private readonly IBotSettingService _botSettingService;

    public DiscordService(DiscordSocketClient discordSocketClient,
        IConfiguration config,
        ITextCommandHandler commandHandler,
        IInteractionHandler interactionHandler,
        ILoggingService loggingService,
        IBotSettingService botSettingService)
    {
        _client = discordSocketClient;
        _config = config;
        _commandHandler = commandHandler;
        _interactionHandler = interactionHandler;
        _loggingService = loggingService;
        _botSettingService = botSettingService;
        
        _client.Log += loggingService.LogAsync;
        _client.Ready += OnReady;
    }

    public async Task StartAsync()
    {
        var botSetting = await _botSettingService.GetBotSettingAsync();
        await _client.LoginAsync(TokenType.Bot, botSetting!.Token);
        await _client.StartAsync();

        _ = Task.Run(async () => await MuteHelper.MuteWorker(_client));
    }

    public async Task StopAsync()
    {
        await _client.LogoutAsync();
        await _client.StopAsync();
    }

    private async Task OnReady()
    {
        await _commandHandler.InitializeAsync();
        await _interactionHandler.InitializeAsync();
    }
}
