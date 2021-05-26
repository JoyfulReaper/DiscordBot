/*
MIT License

Copyright(c) 2021 Kyle Givler
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
using DiscordBot.Helpers;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class AutoRoles : ModuleBase<SocketCommandContext>
    {
        private readonly IAutoRoleService _autoRoleService;
        private readonly IServerService _servers;
        private readonly ILogger<AutoRoles> _logger;

        public AutoRoles(IAutoRoleService autoRoleService,
            IServerService servers,
            ILogger<AutoRoles> logger)
        {
            _autoRoleService = autoRoleService;
            _servers = servers;
            _logger = logger;
        }

        [Command("autoroles", RunMode = RunMode.Async)]
        [Summary("Show autoroles")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ShowAutoRoles()
        {
            await Context.Channel.TriggerTypingAsync();

            var autoRoles = await _autoRoleService.GetAutoRoles(Context.Guild);
            if(autoRoles.Count == 0)
            {
                await ReplyAsync("This server does not yet have any autoroles!");
                return;
            }

            string description = "This message lists all available autoroles.\n in order to remove and autorole, use the name or ID";
            foreach (var role in autoRoles)
            {
                description += $"\n{role.Mention} ({role.Id})";
            }

            await ReplyAsync(description);
        }

        [Command("addautorole", RunMode = RunMode.Async)]
        [Summary("Add an autorole")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddAutoRole([Summary("Name of the autorole to add")][Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRoleService.GetAutoRoles(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if(role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if(role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher postion than that bot!");
                return;
            }

            if(autoRoles.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already an autorole!");
                return;
            }

            await _autoRoleService.AddAutoRole(Context.Guild.Id, role.Id);
            //await ReplyAsync($"The role {role.Mention} had been added to the autoroles!");
            await Context.Channel.SendEmbedAsync("Auto Role added", 
                "The role {role.Mention} had been added to the autoroles!",
                await _servers.GetEmbedColor(Context.Guild.Id));
            await _servers.SendLogsAsync(Context.Guild, "Auto Role Added", $"{Context.User.Mention} added {role.Mention} to the Auto Roles!");
            _logger.LogInformation("{user} added {role} to the auto roles for {server}",
                Context.User.Username, role.Name, Context.Guild.Name);
        }

        [Command("delautorole", RunMode = RunMode.Async)]
        [Summary("Delete an autorole")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelAutoRole([Summary("Name of auto role to delete")][Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var autoRoles = await _autoRoleService.GetAutoRoles(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (autoRoles.Any(x => x.Id != role.Id))
            {
                await ReplyAsync("That role is not a autorole yet!");
                return;
            }

            await _autoRoleService.RemoveAutoRole(Context.Guild.Id, role.Id);
            await ReplyAsync($"The autorole {role.Mention} has been removed from the autoroles!");
            
            await _servers.SendLogsAsync(Context.Guild, "Auto role removed", $"{Context.User} removed the auto role {role.Mention}.");
            _logger.LogInformation("{user} removed {role} from the autoroles in {server}",
                Context.User.Username, role.Name, Context.Guild.Name);
        }

        [Command("runautoroles", RunMode = RunMode.Async)]
        [Summary("Assign auto roles to all users")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RunAutoRoles()
        {
            await ReplyAsync("Please wait, this will hit API rate limiting...");
            await Context.Channel.TriggerTypingAsync();

            var autoRoles = await _autoRoleService.GetAutoRoles(Context.Guild);
            if(autoRoles.Count == 0)
            {
                await ReplyAsync("No auto roles exists!");
            }

            foreach (var user in Context.Guild.Users)
            {
                await user.AddRolesAsync(autoRoles);
            }

            await ReplyAsync("AutoRoles have been added!");
            await _servers.SendLogsAsync(Context.Guild, "Autoroles run", $"{Context.User} assigned the auto roles to all users on the server.");
            _logger.LogInformation("{user} assinged auto roles to all usered on {server}",
                Context.User.Username, Context.Guild.Name);
        }
    }
}
