using Discord;
using Discord.Commands;
using DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class AutoRoles : ModuleBase<SocketCommandContext>
    {
        private readonly AutoRoleService _autoRoleService;
        private readonly IServerService _servers;

        public AutoRoles(AutoRoleService autoRoleService,
            IServerService servers)
        {
            _autoRoleService = autoRoleService;
            _servers = servers;
        }

        [Command("autoroles", RunMode = RunMode.Async)]
        [Summary("show autoroles")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ShowRanks()
        {
            var autoRoles = await _autoRoleService.GetAutoRoles(Context.Guild);
            if(autoRoles.Count == 0)
            {
                await ReplyAsync("This server does not yet have any autoroles!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string description = "This message lists all available autoroles.\n in order to remove and autorole, use the name or ID";
            foreach (var role in autoRoles)
            {
                description += $"\n{role.Mention} ({role.Id})";
            }

            await ReplyAsync(description);
        }

        [Command("addautorole", RunMode = RunMode.Async)]
        [Summary("add an autorole")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddRank([Remainder] string name)
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
            await ReplyAsync($"The role {role.Mention} had been added to the autoroles!");
        }

        [Command("delautorole", RunMode = RunMode.Async)]
        [Summary("Delete an autorole")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelRank([Remainder] string name)
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
        }
    }
}
