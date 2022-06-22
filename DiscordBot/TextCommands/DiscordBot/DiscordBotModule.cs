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
using Discord.Commands;
using Discord.Interactions;
using DiscordBot.TextCommands.Infrastructure;
using DiscordBotLibrary.ConfigSections;
using DiscordBotLibrary.Extensions;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using ContextType = Discord.Commands.ContextType;
using RequireContextAttribute = Discord.Commands.RequireContextAttribute;
using RequireOwnerAttribute = Discord.Commands.RequireOwnerAttribute;
using SummaryAttribute = Discord.Commands.SummaryAttribute;

namespace DiscordBot.Commands.DiscordBot;

public class DiscordBotModule : DiscordBotModuleBase<SocketCommandContext>
{
    private readonly InteractionService _interactionService;
    private readonly IConfiguration _config;
    private readonly IGuildService _guildService;
    private readonly OwnerInformation _ownerInformation;
    private readonly BotInformation _botInformation;

    public DiscordBotModule(InteractionService interactionService,
        IConfiguration config,
        IGuildService guildService) : base (guildService)
    {
        _interactionService = interactionService;
        _config = config;
        _guildService = guildService;
        _ownerInformation = _config.GetSection("OwnerInformation").Get<OwnerInformation>();
        _botInformation = _config.GetSection("BotInformation").Get<BotInformation>();
    }
    
    [Command("prefix")]
    [Discord.Commands.RequireUserPermission(GuildPermission.Administrator)]
    [Summary("Change the prefix")]
    public async Task Prefix([Summary("The prefix to use to address the bot")] string? prefix = null)
    {
        await Context.Channel.TriggerTypingAsync();
        var myPrefix = await _guildService.GetGuildPrefixAsync(Context.Guild.Id);

        if (prefix == null)
        {
            await ReplyAsync(embed: EmbedHelper.GetEmbed("Prefix", $"My Prefix is {myPrefix}",
                await _guildService.GetEmbedColorAsync(Context.Guild.Id), ImageLookup.GetImageUrl(nameof(ImageLookup.PREFIX_IMAGES))));

            return;
        }

        if (prefix.Length > _botInformation.PrefixMaxLength)
        {
            await ReplyAsync(embed: EmbedHelper.GetEmbed("Invalid Prefix", $"Prefix must be less than {_botInformation.PrefixMaxLength} characters.",
                await _guildService.GetEmbedColorAsync(Context.Guild.Id), ImageLookup.GetImageUrl(nameof(ImageLookup.PREFIX_IMAGES))));

            return;
        }

        await _guildService.SetGuildPrefixAsync(Context.Guild.Id, prefix);
        await ReplyAsync(embed: EmbedHelper.GetEmbed("Prefix Changed", $"My Prefix is now `{prefix}`",
            await _guildService.GetEmbedColorAsync(Context.Guild.Id), ImageLookup.GetImageUrl(nameof(ImageLookup.PREFIX_IMAGES))));

        await _guildService.SendLogsAsync(Context.Guild, "Prefix adjusted", $"{Context.User.Mention} modifed the prefix to {prefix}");
    }

    [Command("registerSlashGlobal")]
    [Summary("Registers slash commands")]
    [RequireOwner]
    [Alias("rsg")]
    public async Task RegisterSlashCommandsGlobal()
    {
        await _interactionService.RegisterCommandsGloballyAsync(true);
        await ReplyAsync("Registered slash commands globally!");
        await SendLogsAsync("Slash commands registered", $"{Context.User.GetDisplayName()} registered slash commands globally!");
    }

    [Command("registerSlashDev")]
    [Summary("Registers slash commands to developmnent guild")]
    [RequireOwner]
    [RequireContext(ContextType.Guild)]
    [Alias("rsd")]
    public async Task RegisterSlashCommandsDev()
    {
        await _interactionService.RegisterCommandsToGuildAsync(_ownerInformation.DevGuild, true);
        await ReplyAsync("Registered slash commands to development guild!");
        await SendLogsAsync("Slash commands registered", $"{Context.User.GetDisplayName()} registered slash commands in the development guild!");
    }

    [Command("unregisterSlashDev")]
    [Summary("Un-registers slash commands to developmnent guild")]
    [RequireOwner]
    [Alias("usd")]
    public async Task UnRegisterSlashCommandsDev()
    {
        var interactionModules = from type in Assembly.GetEntryAssembly().GetTypes()
                      where typeof(InteractionModuleBase<SocketInteractionContext>).IsAssignableFrom(type)
                      select type;
        
        foreach(var type in interactionModules)
        {
            await _interactionService.RemoveModuleAsync(type);
        }

        await _interactionService.RegisterCommandsToGuildAsync(_ownerInformation.DevGuild, true);
        await ReplyAsync("un-Registered slash commands to development guild!");
        await SendLogsAsync("Slash commands ub-registered", $"{Context.User.GetDisplayName()} un-registered slash commands in the development guild!");
    }
}
