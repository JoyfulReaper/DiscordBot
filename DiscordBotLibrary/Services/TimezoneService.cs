﻿using DiscordBotLibrary.Models;
using DiscordBotLibrary.Repositories;
using DiscordBotLibrary.Repositories.Interfaces;
using DiscordBotLibrary.Services.Interfaces;

namespace DiscordBotLibrary.Services;
public class TimezoneService : ITimezoneService
{
    private readonly IUserTimezoneRepository _userTimezoneRepository;

    public TimezoneService(IUserTimezoneRepository userTimezoneRepository)
    {
        _userTimezoneRepository = userTimezoneRepository;
    }

    public async Task<UserTimezone> LoadUserTimeZoneAsync(long userTimezoneId)
    {
        return await _userTimezoneRepository.LoadUserTimeZoneAsync(userTimezoneId);
    }

    public async Task SaveUserTimezoneAsync(UserTimezone userTimezone)
    {
        await _userTimezoneRepository.SaveUserTimezoneAsync(userTimezone);
    }
}
