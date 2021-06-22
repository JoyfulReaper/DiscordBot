using Discord.Addons.Interactive;
using Discord.Commands;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [Name("HelpHidden")]
    public class HelpModule : InteractiveBase
    {
        private readonly CommandService _commandService;
        private readonly ILogger<HelpModule> _logger;
        private readonly IServerService _servers;

        public HelpModule(CommandService commandService,
            ILogger<HelpModule> logger,
            IServerService servers)
        {
            _commandService = commandService;
            _logger = logger;
            _servers = servers;
        }

        [Command("help")]
        [Summary("Get some help!")]
        public async Task HelpCommand()
        {
            // Changing pages seems a little broken when DMing the bot, TODO: Look into later
            string prefix = string.Empty;
            if(Context.Guild != null)
            {
                prefix = await _servers.GetGuildPrefix(Context.Guild.Id);
            }

            List<string> pages = new List<string>();

            foreach(var module in _commandService.Modules)
            {
                if(module.Name.EndsWith("Hidden"))
                {
                    continue;
                }
                string page = $"Command Module: ***{module.Name}***\n";
                foreach(var command in module.Commands)
                {
                    page += $"`{prefix}{command.Name}` - {command.Summary ?? "No description provided"}\n";
                }
                pages.Add(page);
            }

            await PagedReplyAsync(pages);

            _logger.LogInformation("{username}#{discriminator} executed help on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);
        }
    }
}
