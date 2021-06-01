using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.DataAccess;
using DiscordBot.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;

// TODO Remove some of the duplicated code
// TODO Add logging
// TODO Use embeds

namespace DiscordBot.Commands
{
    public class TimeZone : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<TimeZone> _logger;
        private readonly IUserTimeZonesRepository _userTimeZones;

        public TimeZone(ILogger<TimeZone> logger,
            IUserTimeZonesRepository userTimeZones)
        {
            _logger = logger;
            _userTimeZones = userTimeZones;
        }

        [Command("registertimezone")]
        [Alias("registertz")]
        [Summary("Register your timezone with the bot")]
        public async Task RegisterTimeZone([Remainder] string timeZone = null)
        {
            if (timeZone == null)
            {
                await ReplyAsync("Please provide a windows or IANA timezone");
                return;
            }

            TimeZoneInfo tzi;
            if (!TZConvert.TryGetTimeZoneInfo(timeZone, out tzi))
            {
                await ReplyAsync($"{timeZone} is not a valid timezone");
            }
            else
            {
                var userTimeZone = await _userTimeZones.GetByUserID(Context.User.Id);
                if (userTimeZone == null)
                {
                    await _userTimeZones.AddAsync(new UserTimeZone
                    {
                        UserId = Context.User.Id,
                        TimeZone = timeZone,
                    });

                    await _userTimeZones.AddAsync(userTimeZone);
                    await ReplyAsync("Registered!");
                }
                else
                {
                    userTimeZone.TimeZone = timeZone;
                    await _userTimeZones.EditAsync(userTimeZone);
                    await ReplyAsync("Updated!");
                }
            }
        }

        [Command("userstime")]
        [Alias("usertime")]
        [Summary("Get the time for a given user")]
        public async Task UserTime([Remainder] SocketUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }

            var userTimeZone = await _userTimeZones.GetByUserID(user.Id);
            if(userTimeZone == null)
            {
                await ReplyAsync($"{user.Username} has not registered their timezone!");
                return;
            }

            TimeZoneInfo tzi;
            if (!TZConvert.TryGetTimeZoneInfo(userTimeZone.TimeZone, out tzi))
            {
                await ReplyAsync($"{userTimeZone.TimeZone} is not a valid timezone!");
                return;
            }

            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
            await ReplyAsync($"{user.Username}'s Date and time is: {time}");
        }

        [Command("validtimezone")]
        [Summary("Validate a windows or IANA timezone")]
        public async Task ValidTimeZone([Remainder] string timeZone = null)
        {
            if (timeZone == null)
            {
                await ReplyAsync("Please provide a windows or IANA timezone");
                return;
            }

            TimeZoneInfo tzi;
            if (!TZConvert.TryGetTimeZoneInfo(timeZone, out tzi))
            {
                await ReplyAsync($"{timeZone} is not a valid timezone");
            }
            else
            {
                await ReplyAsync($"{tzi} is a valid timezone");
            }
        }

        [Command("time")]
        [Summary("Get the time in the given timezone")]
        public async Task GetTime([Remainder] string timeZone = null)
        {
            if(timeZone == null)
            {
                await ReplyAsync("Please provide a windows or IANA timezone");
                return;
            }

            TimeZoneInfo tzi;
            if (!TZConvert.TryGetTimeZoneInfo(timeZone, out tzi))
            {
                await ReplyAsync($"{timeZone} is not a valid timezone");
            }
            else
            {
                DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
                await ReplyAsync($"The time is: {time}");
            }
        }
    }
}
