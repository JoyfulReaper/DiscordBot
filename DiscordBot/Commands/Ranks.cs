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
using DiscordBot.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Ranks : ModuleBase<SocketCommandContext>
    {
        private readonly IRankService _rankService;
        private readonly IServerService _servers;

        public Ranks(IRankService rankService,
            IServerService servers)
        {
            _rankService = rankService;
            _servers = servers;
        }

        [Command("ranks", RunMode = RunMode.Async)]
        [Summary("show ranks")]
        public async Task ShowRanks()
        {
            var ranks = await _rankService.GetRanks(Context.Guild);
            if(ranks.Count == 0)
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
        [Summary("add a rank")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task AddRank([Remainder] string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _rankService.GetRanks(Context.Guild);

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

            if(ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is already a rank!");
                return;
            }

            await _rankService.AddRank(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} had been added to the ranks!");
        }

        [Command("delrank", RunMode = RunMode.Async)]
        [Summary("Delete a rank")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task DelRank([Remainder]string name)
        {
            await Context.Channel.TriggerTypingAsync();
            var ranks = await _rankService.GetRanks(Context.Guild);

            var role = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
            if (role == null)
            {
                await ReplyAsync("That role does not exist!");
                return;
            }

            if (!ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That role is not a rank yet!");
                return;
            }

            await _rankService.RemoveRank(Context.Guild.Id, role.Id);
            await ReplyAsync($"The role {role.Mention} has been removed from the ranks!");
        }

        [Command("rank", RunMode = RunMode.Async)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Rank([Remainder]string identifier = null)
        {
            await Context.Channel.TriggerTypingAsync();

            if(identifier == null)
            {
                await ReplyAsync("Please specifiy the rank to add/remove");
                return;
            }

            var ranks = await _rankService.GetRanks(Context.Guild);

            IRole role;

            if(ulong.TryParse(identifier, out ulong roleId))
            {
                var roleById = Context.Guild.Roles.FirstOrDefault(x => x.Id == roleId);
                if(roleById == null)
                {
                    await ReplyAsync("That roles does not exist!");
                    return;
                }

                role = roleById;
            }
            else
            {
                var roleByName = Context.Guild.Roles.FirstOrDefault(x => string.Equals(x.Name, identifier, StringComparison.CurrentCultureIgnoreCase));
                if(roleByName == null)
                {
                    await ReplyAsync("That role does not exists!");
                    return;
                }

                role = roleByName;
            }

            if (!ranks.Any(x => x.Id == role.Id))
            {
                await ReplyAsync("That rank does not exist!");
                return;
            }

            if((Context.User as SocketGuildUser).Roles.Any(x => x.Id == role.Id))
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
