using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeZoneConverter;

namespace DiscordBot.Commands
{
    public class TimeZone : ModuleBase<SocketCommandContext>
    {
        [Command("validtimezone")]
        [Summary("Validate a windows or IANA timezone")]
        public async Task ValidTimeZone([Remainder] string timeZone = null)
        {
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
