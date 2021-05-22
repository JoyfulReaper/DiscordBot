using Discord.Addons.Interactive;
using Discord.Commands;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Help : InteractiveBase
    {
        private readonly CommandService _commandService;
        private readonly ILogger<Help> _logger;
        private readonly IServerService _servers;

        public Help(CommandService commandService,
            ILogger<Help> logger,
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
            var prefix = await _servers.GetGuildPrefix(Context.Guild.Id);

            List<string> pages = new List<string>();

            foreach(var module in _commandService.Modules)
            {
                string page = $"Command Module: ***{module.Name}***\n";
                foreach(var command in module.Commands)
                {
                    page += $"`{prefix}{command.Name}` - {command.Summary ?? "No description provided"}\n";
                }
                pages.Add(page);
            }

            await PagedReplyAsync(pages);
        }
    }
}
