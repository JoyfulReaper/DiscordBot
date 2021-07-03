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
using Discord.WebSocket;
using DiscordBotLib.Helpers;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [Name("Ranks and Auto Roles")]
    public class RanksAndAutoRolesModule : ModuleBase<SocketCommandContext>
    {
        private readonly IAutoRoleService _autoRoleService;
        private readonly IServerService _servers;
        private readonly ILogger<RanksAndAutoRolesModule> _logger;
        private readonly IRankService _rankService;

        public RanksAndAutoRolesModule(IAutoRoleService autoRoleService,
            IServerService servers,
            ILogger<RanksAndAutoRolesModule> logger,
            IRankService rankService)
        {
            _autoRoleService = autoRoleService;
            _servers = servers;
            _logger = logger;
            _rankService = rankService;
        }

        [Command("autoroles", RunMode = RunMode.Async)]
        [Summary("Show autoroles")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ShowAutoRoles()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed autoroles on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var autoRoles = await _autoRoleService.GetAutoRoles(Context.Guild);
            if (autoRoles == null || !autoRoles.Any())
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

            _logger.LogInformation("{username}#{discriminator} executed addautorole ({role}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, name, Context.Guild?.Name ?? "DM", Context.Channel.Name);

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

            if(autoRoles != null && autoRoles.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already an autorole!");
                return;
            }

            await _autoRoleService.AddAutoRole(Context.Guild.Id, role.Id);

            await Context.Channel.SendEmbedAsync("Auto Role added", 
                $"The role {role.Mention} had been added to the autoroles!",
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

            _logger.LogInformation("{username}#{discriminator} executed delautorole ({role}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, name, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            var autoRoles = await _autoRoleService.GetAutoRoles(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (autoRoles.All(x => x.Id != role.Id))
            {
                await ReplyAsync("That role is not a autorole yet!");
                return;
            }

            await _autoRoleService.RemoveAutoRole(Context.Guild.Id, role.Id);

            await Context.Channel.SendEmbedAsync("Auto Role added", $"The role {role.Mention} had been remove from the auto roles!", await _servers.GetEmbedColor(Context.Guild.Id));

            await _servers.SendLogsAsync(Context.Guild, "Auto role removed", $"{Context.User.Mention} removed the auto role {role.Mention}.");

            _logger.LogInformation("{user} removed {role} from the autoroles in {server}",
                Context.User.Username, role.Name, Context.Guild.Name);
        }

        [Command("runautoroles", RunMode = RunMode.Async)]
        [Summary("Assign auto roles to all users")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task RunAutoRoles()
        {
            _logger.LogInformation("{username}#{discriminator} executed runautoroles on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            await ReplyAsync("Please wait, this will hit API rate limiting...");
            await Context.Channel.TriggerTypingAsync();

            var autoRoles = await _autoRoleService.GetAutoRoles(Context.Guild);
            if(!autoRoles.Any())
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

        [Command("ranks", RunMode = RunMode.Async)]
        [Summary("Show available ranks")]
        public async Task ShowRanks()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} invoked ranks on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);

            if (await ServerHelper.CheckIfContextIsDM(Context))
            {
                return;
            }

            var ranks = await _rankService.GetRanks(Context.Guild);
            if (ranks.Count == 0)
            {
                await ReplyAsync("This server does not yet have any ranks!");
                return;
            }

            await Context.Channel.TriggerTypingAsync();

            string description = "This message lists all available ranks, you can use the name or ID of the rank.";
            foreach (var rank in ranks)
            {
                description += $"\n{rank.Mention} ({rank.Id})";
            }

            await ReplyAsync(description);
        }

        [Command("addrank", RunMode = RunMode.Async)]
        [Summary("Add a rank")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddRank([Summary("Name of the rank to add")][Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _rankService.GetRanks(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await ReplyAsync("That role has a higher postion than that bot!");
                return;
            }

            if (ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already a rank!");
                return;
            }

            await _rankService.AddRank(Context.Guild.Id, role.Id);
            //await ReplyAsync($"The role {role.Mention} had been added to the ranks!");
            await Context.Channel.SendEmbedAsync("Rank added", $"The role {role.Mention} had been added to the ranks!", await _servers.GetEmbedColor(Context.Guild.Id));

            await _servers.SendLogsAsync(Context.Guild, "Rank Added", $"{Context.User.Mention} added {role.Mention} to the ranks!");
            _logger.LogInformation("{user} added {role} to the ranks for {server}",
                Context.User.Username, role.Name, Context.Guild.Name);
        }

        [Command("delrank", RunMode = RunMode.Async)]
        [Summary("Delete a rank")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelRank([Summary("Name of the rank to delete")][Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _rankService.GetRanks(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (ranks.All(x => x.Id != role.Id))
            {
                await ReplyAsync("That role is not a rank yet!");
                return;
            }

            await _rankService.RemoveRank(Context.Guild.Id, role.Id);
            //await ReplyAsync($"The role {role.Mention} has been removed from the ranks!");
            await Context.Channel.SendEmbedAsync("Rank Removed", $"The role {role.Mention} had been removed from the ranks!", await _servers.GetEmbedColor(Context.Guild.Id));

            await _servers.SendLogsAsync(Context.Guild, "Rank removed", $"{Context.User.Mention} removed the rank {role.Mention}.");
            _logger.LogInformation("{user} removed {role} from the ranks in {server}",
           Context.User.Username, role.Name, Context.Guild.Name);
        }

        [Command("rank", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [Summary("Assign yourself a rank")]
        public async Task Rank([Summary("The rank to assign yourself")][Remainder] string rank = null)
        {
            await Context.Channel.TriggerTypingAsync();

            if (rank == null)
            {
                await ReplyAsync("Please specifiy the rank to add/remove");
                return;
            }

            var ranks = await _rankService.GetRanks(Context.Guild);

            IRole role;

            if (ulong.TryParse(rank, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if (roleById == null)
                {
                    await ReplyAsync("That roles does not exist!");
                    return;
                }

                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, rank, StringComparison.CurrentCultureIgnoreCase));
                if (roleByName == null)
                {
                    await ReplyAsync("That role does not exists!");
                    return;
                }

                role = roleByName;
            }

            if (ranks.All(x => x.Id != role.Id))
            {
                await ReplyAsync("That rank does not exist!");
                return;
            }

            if ((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
            {
                await (Context.User as SocketGuildUser).RemoveRoleAsync(role);
                await ReplyAsync($"Successfully removed the rank {role.Mention} from you.");
                return;
            }

            await (Context.User as SocketGuildUser).AddRoleAsync(role);
            await ReplyAsync($"Successfully added the rank {role.Mention} to you.");
        }
    }
}
