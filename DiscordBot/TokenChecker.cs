using DiscordBotLibrary.Models;
using DiscordBotLibrary.Services.Interfaces;


namespace DiscordBot;

internal class TokenChecker
{
    private readonly IBotSettingService _botSetting;
    private readonly ILoggingService _logger;
    private readonly IBotSettingService _botSettingService;

    public TokenChecker(IBotSettingService botSetting,
        ILoggingService logger,
        IBotSettingService botSettingService)
    {
        _botSetting = botSetting;
        _logger = logger;
        _botSettingService = botSettingService;
    }
    
    internal async Task CheckForTokenAsync()
    {
        BotSetting botSetting = await _botSetting.GetBotSettingAsync();
        if (string.IsNullOrWhiteSpace(botSetting.Token))
        {
            Console.WriteLine("Please enter the bot's token: ");
            botSetting.Token = Console.ReadLine() ?? string.Empty;

            await _botSetting.SaveBotSettingAsync(botSetting);
        }
    }

    internal async Task ClearTokenAndReCheckAsync()
    {
        var settings = await _botSettingService.GetBotSettingAsync();
        settings.Token = null;
        await _botSetting.SaveBotSettingAsync(settings);
        
        await CheckForTokenAsync();
    }
}
