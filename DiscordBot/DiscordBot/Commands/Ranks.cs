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
    public class Ranks : ModuleBase<SocketCommandContext>
    {
        private readonly RankService _rankService;
        private readonly IServerService _servers;

        public Ranks(RankService rankService,
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
                await ReplyAsync("That role has a higher postion thank that bot!");
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
    }
}
