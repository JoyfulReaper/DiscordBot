using Discord.Addons.Interactive;
using Discord.Commands;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
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
        [Alias("halp")]
        [Summary("Get some help!")]
        public async Task HelpCommand([Summary("The command to get help with")][Remainder]string command = null)
        {
            if (command != null)
            {
                var cmd = _commandService.Commands.Where(x => x.Name.ToLowerInvariant() == command.ToLowerInvariant() || x.Aliases.Contains(command.ToLowerInvariant())).SingleOrDefault();
                if(cmd == null)
                {
                    await ReplyAsync($"No such command: `{command}`!");
                    return;
                }

                string output = string.Empty;
                output = $"Name: {cmd.Name}\nAliases: ";
                for(int i = 0; i < cmd.Aliases.Count; i++)
                {
                    output += $"{cmd.Aliases[i]}";
                    if(i != cmd.Aliases.Count -1)
                    {
                        output += ", ";
                    }
                }
                output += $"\nSummary: {cmd.Summary}";

                if (cmd.Parameters.Count > 0)
                {
                    output += "\nParameters: ";
                    for (int i = 0; i < cmd.Parameters.Count; i++)
                    {
                        output += $"{cmd.Parameters[i]}";

                        if (!string.IsNullOrWhiteSpace(cmd.Parameters[i].Summary))
                        {
                            output += $" ({cmd.Parameters[i].Summary})";
                        }

                        if (i != cmd.Parameters.Count - 1)
                        {
                            output += ", ";
                        }
                    }
                }

                await ReplyAsync(output);
                return;
            }


            // Changing pages seems a little broken when DMing the bot, TODO: Look into later
            string prefix = string.Empty;
            if(Context.Guild != null)
            {
                prefix = await _servers.GetGuildPrefix(Context.Guild.Id);
            }

            List<string> pages = new List<string>();

            foreach(var module in _commandService.Modules)
            {
                if (module.Name.EndsWith("Hidden"))
                {
                    continue;
                }
                string page = $"Command Module: ***{module.Name}***\n";
                foreach(var cmd in module.Commands)
                {
                    page += $"`{prefix}";
                    if (module.Group != null)
                    {
                        page += $"{module.Group} ";
                    }
                    page += $"{cmd.Name}` - {cmd.Summary ?? "No description provided"}\n";
                }
                pages.Add(page);
            }

            await PagedReplyAsync(pages);

            _logger.LogInformation("{username}#{discriminator} executed help on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);
        }
    }
}
