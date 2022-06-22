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

using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DiscordBot;
internal class DiscordBotApp
{
    private readonly IConfiguration _config;
    private readonly IDiscordService _discordService;
    private readonly ILoggingService _loggingService;
    private readonly TokenChecker _tokenChecker;

    public DiscordBotApp(IConfiguration config,
        IDiscordService discordService,
        ILoggingService loggingService,
        TokenChecker tokenChecker)
    {
        _config = config;
        _discordService = discordService;
        _loggingService = loggingService;
        _tokenChecker = tokenChecker;
        _loggingService.OnBadToken += OnBadToken;
    }
    
    internal async Task StartAsync()
    {
        ConsoleHelper.ColorWriteLine(ConsoleColor.Red, $"{_config["BotInformation:BotName"]}");
        ConsoleHelper.ColorWriteLine(ConsoleColor.Blue, $"MIT License\n\nCopyright(c) 2022 Kyle Givler (JoyfulReaper)\n{_config["Botinformation:BotWebsite"]}\n\n");

        await _tokenChecker.CheckForTokenAsync();

        await _discordService.StartAsync();
    }

    internal async Task StopAsync()
    {
        _loggingService.OnBadToken -= OnBadToken;
        await _discordService.StopAsync();
        Environment.Exit(0);
    }
    
    private async void OnBadToken(object? sender, EventArgs e)
    {
        await _discordService.StopAsync();
        await Task.Delay(250);
        await _tokenChecker.ClearTokenAndReCheckAsync();
        await _discordService.StartAsync();
    }
}
