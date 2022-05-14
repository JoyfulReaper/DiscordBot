using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Models;
using DiscordBotLibrary.Services.Interfaces;

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

    [SlashCommand("usertime", "Get the time for a given user")]
    public async Task GetUserTimeSlash(IUser user)
    {
        await RespondWithUserTime(user);
    }

    [UserCommand("usertime")]
    public async Task GetUserTime(IUser user)
    {
        await RespondWithUserTime(user);
    }

    [SlashCommand("valid", "Validate a windows or IANA timezone")]
    public async Task ValidTimeZone(string timezone)
    {
        await Context.Channel.TriggerTypingAsync();

        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timezone);
            await RespondAsync(embed: EmbedHelper.GetEmbed("Invalid Time Zone", $"`{timezone}` *is* a valid windows or IANA timezone.",
                await _guildService.GetEmbedColorAsync(Context)));
        }
        catch (TimeZoneNotFoundException)
        {
            await RespondAsync(embed: EmbedHelper.GetEmbed("Invalid Time Zone", $"`{timezone}` is *not* a valid windows or IANA timezone.",
                await _guildService.GetEmbedColorAsync(Context)));
        }
    }

    [SlashCommand("timezone", "Get the time in the given timezone")]
    public async Task GetTime(string timezone)
    {
        await Context.Channel.TriggerTypingAsync();

        try
        {
            TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);

            await RespondAsync(embed: EmbedHelper.GetEmbed($"Current Time", $"The current time in {timezone} is:\n`{time}`",
                await _guildService.GetEmbedColorAsync(Context)));

        }
        catch(TimeZoneNotFoundException)
        {
            await RespondAsync(embed: EmbedHelper.GetEmbed("Invalid Time Zone", $"`{timezone}` is *not* a valid windows or IANA timezone.",
                await _guildService.GetEmbedColorAsync(Context)));
        }
    }

    private async Task RespondWithUserTime(IUser user)
    {
        await Context.Channel.TriggerTypingAsync();

        var userDb = await _userService.LoadUserAsync(user.Id);
        var userTimeZone = await _timezoneService.LoadUserTimeZoneAsync(userDb.UserId);
        string userName = "";

        var socketGuildUser = user as SocketGuildUser;
        if (socketGuildUser != null)
        {
            userName = socketGuildUser.DisplayName;
        }
        else
        {
            userName = user.Username;
        }

        if (userTimeZone == null || userTimeZone.TimeZone == null)
        {
            await RespondAsync(embed: EmbedHelper.GetEmbed("Not Registered", $"{userName} has not registered their time zone.",
               await _guildService.GetEmbedColorAsync(Context)));

            return;
        }

        try
        {
            TimeZoneInfo tzi = TimeZoneInfo.FromSerializedString(userTimeZone.TimeZone);

            DateTime time = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
            await RespondAsync(embed: EmbedHelper.GetEmbed($"{userName}'s Time", $"The date and time for {userName} is:\n`{time}`",
                await _guildService.GetEmbedColorAsync(Context)));
        }
        catch (TimeZoneNotFoundException)
        {
            await RespondAsync(embed: EmbedHelper.GetEmbed("Invalid Timezone", $"{userName} has some how managed to register an invalid timezone!",
                await _guildService.GetEmbedColorAsync(Context)));

            return;
        }
    }

}
