using Discord;
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
