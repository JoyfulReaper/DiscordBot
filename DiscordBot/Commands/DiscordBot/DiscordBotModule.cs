using Discord.Commands;
using Discord.Interactions;
using DiscordBotLibrary.ConfigSections;
using Microsoft.Extensions.Configuration;
using RequireOwnerAttribute = Discord.Commands.RequireOwnerAttribute;
using SummaryAttribute = Discord.Commands.SummaryAttribute;

namespace DiscordBot.Commands.DiscordBot;

public class DiscordBotModule : ModuleBase<SocketCommandContext>
{
    private readonly InteractionService _interactionService;
    private readonly IConfiguration _config;
    private readonly OwnerInformation _ownerInformation;

    public DiscordBotModule(InteractionService interactionService,
        IConfiguration config)
    {
        _interactionService = interactionService;
        _config = config;
        _ownerInformation = _config.GetSection("OwnerInformation").Get<OwnerInformation>();
    }

    [Command("registerSlashGlobal")]
    [Summary("Registers slash commands")]
    [RequireOwner]
    [Alias("rsg")]
    public async Task RegisterSlashCommandsGlobal()
    {
        await _interactionService.RegisterCommandsGloballyAsync();
        await ReplyAsync("Registered slash commands globally!");
    }

    [Command("registerSlashDev")]
    [Summary("Registers slash commands to developmnent guild")]
    [RequireOwner]
    [Alias("rsd")]
    public async Task RegisterSlashCommandsDev()
    {
        await _interactionService.RegisterCommandsToGuildAsync(_ownerInformation.DevGuild);
        await ReplyAsync("Registered slash commands to development guild!");
    }
}
