using DiscordBotLibrary.Models;

namespace DiscordBotLibrary.Services.Interfaces;

public interface ITimezoneService
{
    Task<UserTimezone> LoadUserTimeZoneAsync(long userTimezoneId);
    Task SaveUserTimezoneAsync(UserTimezone userTimezone);
}