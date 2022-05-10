using Discord.Interactions;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Models;
using DiscordBotLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Interactions.SlashCommands.General;
[Group("time", "Time commands")]
public class TimeModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildService _guildService;
    private readonly IUserService _userService;
    private readonly ITimezoneService _timezoneService;

    public TimeModule(IGuildService guildService,
        IUserService userService,
        ITimezoneService timezoneService)
    {
        _guildService = guildService;
        _userService = userService;
        _timezoneService = timezoneService;
    }
    
    [SlashCommand("register", "Register your timezone.")]
    public async Task RegisterTimeZone(string timezone)
    {
        await Context.Channel.TriggerTypingAsync();
        
        try
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);

            var user = await _userService.LoadUserAsync(Context.User.Id);
            var userTimeZone = await _timezoneService.LoadUserTimeZoneAsync(user.UserId);

            if (userTimeZone == null)
            {
                var userTz = new UserTimezone
                {
                    UserId = user.UserId,
                    TimeZone = tz.ToSerializedString(),
                };

                await _timezoneService.SaveUserTimezoneAsync(userTz);
                await RespondAsync(embed: EmbedHelper.GetEmbed("Succesfully Registered", $"Successfully registered your time zone: `{tz.DisplayName}`",
                    await _guildService.GetEmbedColorAsync(Context)));
            }
            else
            {
                userTimeZone.TimeZone = tz.ToSerializedString();
                await _timezoneService.SaveUserTimezoneAsync(userTimeZone);
                await RespondAsync(embed: EmbedHelper.GetEmbed("Succesfully Updated", $"Successfully updated your time zone: `{tz.DisplayName}`",
                    await _guildService.GetEmbedColorAsync(Context)));
            }
        } 
        catch (TimeZoneNotFoundException)
        {
            await RespondAsync($"Timezone: `{timezone}` not found. Please use a vaild Windows or IANA timezone.");
            return;
        }
       
    }

}
