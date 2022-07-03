
using DiscordBotLibrary.Models;

namespace DiscordBotLibrary.Services.Interfaces;

public interface IWarningService
{
    Task ClearWarningsAsync(long id, long guildId);
    Task<IEnumerable<Warning>> GetWarningsAsync(long userId, long guildId);
    Task AddWarningAsync(Warning warning);
    Task<WarningAction?> GetWarningActionAsync(long guildId);
}
