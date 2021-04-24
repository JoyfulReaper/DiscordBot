using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    // TODO add logging

    public class General : ModuleBase
    {
        [Command("ping")]
        public async Task Ping()
        {
            await Context.Channel.SendMessageAsync("Pong!");
        }

        [Command("info")]
        public async Task Info(SocketGuildUser mentionedUser = null)
        {
            if (mentionedUser == null)
            {
                mentionedUser = Context.User as SocketGuildUser;
            }

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(mentionedUser.GetAvatarUrl() ?? mentionedUser.GetDefaultAvatarUrl())
                .WithDescription("User information:")
                .WithColor(new Color(33, 176, 252))
                .AddField("User ID", mentionedUser.Id, true)
                .AddField("Discriminator", mentionedUser.Discriminator, true)
                .AddField("Created at", mentionedUser.CreatedAt.ToString("MM/dd/yyyy"), true)
                .AddField("Joined at", mentionedUser.JoinedAt.Value.ToString("MM/dd/yyyy"), true)
                .AddField("Roles", string.Join(" ", mentionedUser.Roles.Select(x => x.Mention)))
                .WithCurrentTimestamp();

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task Purge(int amount)
        {
            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendMessageAsync($"{messages.Count()} messages deleted successfuly!");
            await Task.Delay(2500);
            await message.DeleteAsync();
        }

        [Command("server")]
        public async Task Server()
        {
            //var test = Context.Guild as SocketGuild;
            //var active = test.Users.Where(x => x.Status == UserStatus.Online);

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Server information:")
                .WithTitle($"{Context.Guild.Name} Information")
                .WithColor(33, 176, 252)
                .AddField("Created at", Context.Guild.CreatedAt.ToString("MM/dd/yyyy"), true)
                .AddField("Member count", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online users", (Context.Guild as SocketGuild).Users.Where(x => x.Status == UserStatus.Offline).Count() + " members", true);

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
    }
}
