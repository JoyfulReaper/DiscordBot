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
using DiscordBot.Interactions.Infrastructure;
using DiscordBotLibrary.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Interactions.SlashCommands.Moderation;

[Group("mod", "Moderation commands")]
public class WarningModule : DiscordBotModuleBase<SocketInteractionContext>
{
    private readonly IGuildService _guildService;
    private readonly IUserService _userService;

    public WarningModule(IGuildService guildService, IUserService userService) : base(guildService)
    {
        _guildService = guildService;
        _userService = userService;
    }

    [SlashCommand("clear", "Clear a users warnings")]
    [RequireUserPermission(GuildPermission.BanMembers)]
    [RequireContext(ContextType.Guild)]
    public async Task ClearWarnings(IUser user)
    {
        await Context.Channel.TriggerTypingAsync();

        var userDb = await _userService.LoadUserAsync(user.Id);
        var guild = await _guildService.LoadGuildAsync(Context.Guild.Id);
        var warnings = await _warningRepository.GetUsersWarnings(server, userDb);

        var cleared = await _warningRepository.ClearUserWarnings(server, userDb);

        await ReplyAsync($"Cleared `{cleared}` warnings for `{user.Username}`");
    }


}
