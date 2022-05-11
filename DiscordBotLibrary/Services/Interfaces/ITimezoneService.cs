using DiscordBotLibrary.Models;

namespace DiscordBotLibrary.Services.Interfaces;

public interface ITimezoneService
{
    Task<UserTimezone> LoadUserTimeZoneAsync(long userId);
    Task SaveUserTimezoneAsync(UserTimezone userTimezone);
}