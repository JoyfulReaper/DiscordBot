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

using Discord.Interactions;
using DiscordBotLibrary.ConfigSections;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Interactions.SlashCommands.Server;

[Group("server", "server related commands")]
public class ServerModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IConfiguration _config;
    private readonly IGuildService _guildService;
    private readonly ILogger<ServerModule> _logger;
    private readonly BotInformation _botInfo;

    public ServerModule(IConfiguration config,
        IGuildService guildService,
        ILogger<ServerModule> logger)
    {
        _config = config;
        _guildService = guildService;
        _logger = logger;
        _botInfo = _config.GetRequiredSection("BotInformation").Get<BotInformation>();
    }

    [SlashCommand("owner", "Retreive the server owner")]   
    public async Task Owner()
    {
        if(Context.Guild == null)
        {
            await RespondAsync(null,
                EmbedHelper.GetEmbedAsArray(_botInfo.BotName, $"DiscordBot was written by JoyfulReaper. Copyright 2022. MIT Licensed.\n{_botInfo.BotWebsite}",
                await _guildService.GetEmbedColorAsync(Context),
                Context.Client.CurrentUser.GetAvatarUrl() ?? Context.Client.CurrentUser.GetDefaultAvatarUrl()));
        }

        if(Context.Guild == null)
        {
            _logger.LogError("Context.Guild was unexpectly null!");
            await RespondAsync("Something went wrong :(");
            return;
        }

        await RespondAsync(null,
            EmbedHelper.GetEmbedAsArray(Context.Guild.Name, $"{Context.Guild.Owner.DisplayName} is the owner of {Context.Guild.Name}",
                await _guildService.GetEmbedColorAsync(Context), Context.Guild.Owner.GetAvatarUrl() ?? Context.Client.CurrentUser.GetDefaultAvatarUrl()));
    }
}
