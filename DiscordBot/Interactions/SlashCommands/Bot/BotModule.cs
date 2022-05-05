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
using DiscordBotLibrary.ConfigSections;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace DiscordBot.Interactions.SlashCommands.Bot;


[Group("bot", "Bot related commands")]
public class BotModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IConfiguration _config;
    private readonly DiscordSocketClient _client;
    private readonly IGuildService _guildService;
    private readonly BotInformation _botInfo;

    public BotModule(IConfiguration configuration,
        DiscordSocketClient client,
        IGuildService guildService)
    {
        _config = configuration;
        _client = client;
        _guildService = guildService;
        _botInfo = _config.GetRequiredSection("BotInformation").Get<BotInformation>();
    }

    [SlashCommand("invite", "Invite the bot to your server")]
    public async Task Invite()
    {
        await RespondAsync(null, EmbedHelper.GetEmbedAsArray("Invite Link", $"Please click on the link to invite {_botInfo.BotName} to your server!\n{_botInfo.InviteLink}",
            await _guildService.GetEmbedColorAsync(Context),
            thumbImage: ImageLookup.GetImageUrl("INVITE_IMAGES")));
    }

    [SlashCommand("uptime", "Get bot uptime and memory usage")]
    public async Task Uptime()
    {
        using var proccess = Process.GetCurrentProcess();
        var memoryMb = proccess.WorkingSet64 / 1024 / 1024;
        var startTime = proccess.StartTime;
        var upTime = DateTime.Now - startTime;

        await RespondAsync(null, EmbedHelper.GetEmbedAsArray("Uptime", $"Uptime: `{upTime}`\nMemory Usage: `{memoryMb} MB`",
            await _guildService.GetEmbedColorAsync(Context)));

    }

    [SlashCommand("servers", "Report the number of servers the bot is in")]
    public async Task Servers()
    {
        await RespondAsync(null, EmbedHelper.GetEmbedAsArray("Servers", $"I am currently in {_client.Guilds.Count} {(_client.Guilds.Count == 1 ? "server" : "servers")}!",
            await _guildService.GetEmbedColorAsync(Context)));
    }

    [SlashCommand("about", "Information about the bot")]
    public async Task About()
    {
        var builder = new EmbedBuilder()
            .WithThumbnailUrl(_client.CurrentUser.GetAvatarUrl() ?? _client.CurrentUser.GetDefaultAvatarUrl())
            .WithDescription($"{_botInfo.BotName}\nMIT License Copyright(c) 2022 JoyfulReaper\n{_botInfo.BotWebsite}\n\n" +
            $"Use `/bot invite` for the link to invite DiscordBot to your server!")
            .WithColor(await _guildService.GetEmbedColorAsync(Context))
            .WithCurrentTimestamp();

        await RespondAsync(null, new Embed[] { builder.Build() });
    }
}
