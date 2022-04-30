using DiscordBotLibrary.Services.Interfaces;

namespace DiscordBotLibrary.Services;

public class DiscordService : IDiscordService
{
    public async Task Start()
    {

    }

    public Task Stop()
    {
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}
