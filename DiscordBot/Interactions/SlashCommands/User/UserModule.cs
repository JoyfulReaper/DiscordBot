/*
MIT License

Copyright(c) 2022 Kyle Givler
https://github.com/JoyfulReaper

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotLibrary.Services.Interfaces;


namespace DiscordBot.Interactions.SlashCommands.User;

[Group("user", "user related commands")]
public class UserModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildService _guildService;

    public UserModule(IGuildService guildService)
    {
        _guildService = guildService;
    }

    [UserCommand("info")]
    public async Task Info(IUser user)
    {
        await RespondWithUserInfo(user);
    }

    [SlashCommand("info", "information about a user")]
    public async Task InfoSlash(IUser user)
    {
        await RespondWithUserInfo(user);
    }

    private async Task RespondWithUserInfo(IUser user)
    {
        await Context.Channel.TriggerTypingAsync();

        var builder = new EmbedBuilder()
            .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
            .WithDescription("User information:")
            .WithColor(await _guildService.GetEmbedColorAsync(Context.Guild?.Id))
            .AddField("User ID", user.Id, true)
            .AddField("Discriminator", user.Discriminator, true)
            .AddField("Created at", user.CreatedAt.ToString("MM/dd/yyyy"), true)
            .WithCurrentTimestamp();

        //TODO: Re-implment timezone stuffs
        //var timezone = await _userTimeZones.GetByUserID(mentionedUser.Id);
        //if (timezone != null)
        //{
        //    builder.AddField("Timezone", timezone.TimeZone, true);
        //}

        SocketGuildUser? guildUser = user as SocketGuildUser;
        if (guildUser != null)
        {
            builder
                .AddField("Joined at", guildUser.JoinedAt?.ToString("MM/dd/yyyy") ?? "(Unkown)", true)
                .AddField("Roles", string.Join(" ", guildUser.Roles.Select(r => r.Name)));
        }

        await RespondAsync(null, new Embed[] { builder.Build() });
    }
}
