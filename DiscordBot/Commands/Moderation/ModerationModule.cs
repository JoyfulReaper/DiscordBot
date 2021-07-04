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
using DiscordBotLib.Services;
using DiscordBotLib.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotLib.Models;
using System;
using DiscordBotLib.DataAccess;
using DiscordBotLib.Enums;

namespace DiscordBot.Commands
{
    [Name("Moderation Commands")]
    public class ModerationModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<ModerationModule> _logger;
        private readonly IServerService _servers;
        private readonly IConfiguration _configuration;
        private readonly IServerRepository _serverRepository;
        private readonly IProfanityRepository _profanityRepository;
        private readonly DiscordSocketClient _client;
        private readonly int _prefixMaxLength;

        public ModerationModule(ILogger<ModerationModule> logger,
            IServerService servers,
            IConfiguration configuration,
            IServerRepository serverRepository,
            IProfanityRepository profanityRepository,
            DiscordSocketClient client)
        {
            _logger = logger;
            _servers = servers;
            _configuration = configuration;
            _serverRepository = serverRepository;
            _profanityRepository = profanityRepository;
            _client = client;
            var prefixConfigValue = _configuration.GetSection("PrefixMaxLength").Value;
            if (int.TryParse(prefixConfigValue, out int maxLength))
            {
                _prefixMaxLength = maxLength;
            }
            else
            {
                _prefixMaxLength = 8;
                _logger.LogError("Unable to set max prefix length, using default: {defaultValue}", _prefixMaxLength);
            }
        }

        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Summary("Ban a user")]
        public async Task Ban(
            [Summary("The user to ban")]SocketGuildUser user, 
            [Summary("The number of days of the banned user's messages to purge")]int days, 
            [Summary("Reason for the ban")][Remainder] string reason = null)
        {
            await Context.Channel.TriggerTypingAsync();

            if(user == null)
            {
                await ReplyAsync("Please provide a user to ban!");
            }

            _logger.LogInformation("{user}#{discriminator} banned {user} in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, user.Username, Context.Channel.Name, Context.Guild?.Name ?? "DM");

            await Context.Channel.SendEmbedAsync("Ban Hammer", $"{user.Mention} has been banned for *{(reason ?? "no reason")}*",
                ColorHelper.GetColor(await _servers.GetServer(Context.Guild)), ImageLookupUtility.GetImageUrl("BAN_IMAGES"));

            await _servers.SendLogsAsync(Context.Guild, "Banned", $"{Context.User.Mention} has banned {user.Mention} and deleted the past {days} day of their messages!");

            await user.BanAsync(days, reason);
        }

        [Command("unban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Summary("unban a user")]
        public async Task Unban([Summary("Id of the user to unban")]ulong userId)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{user}#{discriminator} unbanned {userId} in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, userId, Context.Channel.Name, Context.Guild.Name);

            await Context.Channel.SendEmbedAsync("Un-Banned", $"{userId} has been un-banned",
                ColorHelper.GetColor(await _servers.GetServer(Context.Guild)), ImageLookupUtility.GetImageUrl("UNBAN_IMAGES"));

            await _servers.SendLogsAsync(Context.Guild, "Un-Banned", $"{Context.User.Mention} has un-banned userId: {userId}");

            await Context.Guild.RemoveBanAsync(userId);
        }

        [Command("profanityallow")]
        [Alias("pallow")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Add exception to profanity filter")]
        public async Task ProfanityAllow([Summary("The profanity to allow")][Remainder] string profanity)
        {
            await Context.Channel.TriggerTypingAsync();

            var server = await _servers.GetServer(Context.Guild);
            await _profanityRepository.AllowProfanity(server.GuildId, profanity);

            await Context.Channel.SendEmbedAsync("Profanity Filter", $"Profanity allowed: `{profanity}`",
                ColorHelper.GetColor(server));

            _logger.LogInformation("{user}#{discriminator} invoked profanityallow for {profanity} in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, profanity, Context.Channel.Name, Context.Guild.Name);

            await _servers.SendLogsAsync(Context.Guild, "Profanity Filter", $"Profanity `{profanity}` has been allowed by {Context.User.Mention}");
        }

        [Command("profanityblock")]
        [Alias("pblock")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Add new entry to profanity filter")]
        public async Task ProfanityBlock([Summary("The profanity to block")][Remainder] string profanity)
        {
            await Context.Channel.TriggerTypingAsync();

            var server = await _servers.GetServer(Context.Guild);
            await _profanityRepository.BlockProfanity(server.GuildId, profanity);

            await Context.Channel.SendEmbedAsync("Profanity Filter", $"Profanity blocked: `CENSORED!`",
                ColorHelper.GetColor(server));

            _logger.LogInformation("{user}#{discriminator} invoked profanityblock for {profanity} in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, profanity, Context.Channel.Name, Context.Guild.Name);

            await _servers.SendLogsAsync(Context.Guild, "Profanity Filter", $"Profanity `{profanity}` has been blocked by {Context.User.Mention}");
        }


        [Command("profanityfilter")]
        [Alias("profanity", "pfilter")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Enable or Disable profanity filtering")]
        public async Task ProfanityFilter([Summary("On to allow, censor to censor, delete to delete")] string enabled = null)
        {
            await Context.Channel.TriggerTypingAsync();

            var server = await _servers.GetServer(Context.Guild);

            if (enabled == null)
            {
                var message = "off";
                if (server.ProfanityFilterMode == ProfanityFilterMode.FilterCensor)
                {
                    message = "set to censor";
                }
                else if(server.ProfanityFilterMode == ProfanityFilterMode.FilterDelete)
                {
                    message = "set to delete";
                }
                await ReplyAsync($"Profanity Filter is `{message}`.");
                return;
            }

            if (enabled.ToLowerInvariant() == "censor")
            {
                server.ProfanityFilterMode = ProfanityFilterMode.FilterCensor;
            }
            else if(enabled.ToLowerInvariant() == "delete")
            {
                server.ProfanityFilterMode = ProfanityFilterMode.FilterDelete;
            }
            else if (enabled.ToLowerInvariant() == "off")
            {
                server.ProfanityFilterMode = ProfanityFilterMode.FilterOff;
            }
            else
            {
                await ReplyAsync("Would you like to set the profanity filter to `censor`, `delete` or `off`?");
                return;
            }

            await _serverRepository.EditAsync(server);

            await Context.Channel.SendEmbedAsync("Profanity Filter", $"Profanity Filter has been set to `{enabled}`",
                ColorHelper.GetColor(server));

            await _servers.SendLogsAsync(Context.Guild, "Profanity Filter", $"Profanity Filter has been set to `{enabled}` by {Context.User.Mention}");

            _logger.LogInformation("{user}#{discriminator} set profanityfilter to {enabled} in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, enabled, Context.Channel.Name, Context.Guild.Name);
        }

        [Command("serverinvites")]
        [Alias("allowinvites")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Enable or disable server invites")]
        public async Task ServerInvites([Summary("On to allow, off to disallow")]string enabled = null)
        { 
            var server = await _servers.GetServer(Context.Guild);

            if(enabled == null)
            {
                var message = "off";
                if(server.AllowInvites)
                {
                    message = "on";
                }
                await ReplyAsync($"Server invites are turned {message}.");
                return;
            }

            if (enabled.ToLowerInvariant() == "on")
            {
                server.AllowInvites = true;
            }
            else if(enabled.ToLowerInvariant() == "off")
            {
                server.AllowInvites = false;
            }
            else
            {
                await ReplyAsync("Would you like to turn server invites `on` or `off`?");
                return;
            }

            await Context.Channel.SendEmbedAsync("Server Invites", $"Server invites have been turned {enabled}",
                ColorHelper.GetColor(server));

            await _servers.SendLogsAsync(Context.Guild, "Server Invites", $"Server invites have been turned {enabled} by {Context.User.Mention}");

            _logger.LogInformation("{user}#{discriminator} set serverinvites to {enabled} in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, enabled, Context.Channel.Name, Context.Guild.Name);

            await _serverRepository.EditAsync(server);
        }

        [Command("kick")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [Summary("Kick a user")]
        public async Task Kick([Summary("user to kick")] SocketGuildUser user = null)
        {
            await Context.Channel.TriggerTypingAsync();

            if (user == null)
            {
                await ReplyAsync("Please specify the user the kick");
                return;
            }

            await user.KickAsync();

            await Context.Channel.SendEmbedAsync("Kicked", $"{user.Mention} was kicked to the curb!", 
                ColorHelper.GetColor(await _servers.GetServer(Context.Guild)), ImageLookupUtility.GetImageUrl("KICK_IMAGES"));
            
            await _servers.SendLogsAsync(Context.Guild, "User kicked", $"{Context.User.Mention} kicked {user.Mention}.");

            _logger.LogInformation("{user}#{discriminator} kicked {user} messages in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, user.Username, Context.Channel.Name, Context.Guild.Name);
        }

        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [Summary("Purges the given number of messages from the current channel")]
        public async Task Purge([Summary("The number of message to purge")] int amount)
        {
            await Context.Channel.TriggerTypingAsync();

            var messages = await Context.Channel.GetMessagesAsync(amount + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);

            var message = await Context.Channel.SendEmbedAsync("Purge Successful", $"{messages.Count()} messages deleted successfuly!",
                await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("PURGE_IMAGES"));

            await Task.Delay(3000);
            await message.DeleteAsync();

            await _servers.SendLogsAsync(Context.Guild, "Messages Purged", $"{Context.User.Mention} purged {messages.Count()} messages in {Context.Channel}");

            _logger.LogInformation("{user}#{discriminator} purged {number} messages in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, amount, Context.Channel.Name, Context.Guild.Name);
        }

        [Command("prefix", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Summary("Change the prefix")]
        public async Task Prefix([Summary("The prefix to use to address the bot")]string prefix = null)
        {
            await Context.Channel.TriggerTypingAsync();
            var myPrefix = await _servers.GetGuildPrefix(Context.Guild.Id);

            if (prefix == null)
            {
                await Context.Channel.SendEmbedAsync("Prefix", $"My Prefix is {myPrefix}",
                    await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("PREFIX_IMAGES"));

                return;
            }

            if(prefix.Length > _prefixMaxLength)
            {
                await Context.Channel.SendEmbedAsync("Invalid Prefix",$"Prefix must be less than {_prefixMaxLength} characters.",
                    await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("PREFIX_IMAGES"));

                return;
            }

            await _servers.ModifyGuildPrefix(Context.Guild.Id, prefix);
            await Context.Channel.SendEmbedAsync("Prefix Modified", $"The prefix has been modified to `{prefix}`.",
                     await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("PREFIX_IMAGES"));

            await _servers.SendLogsAsync(Context.Guild, "Prefix adjusted", $"{Context.User.Mention} modifed the prefix to {prefix}");

            _logger.LogInformation("{user}#{discriminator} changed the prefix for {server} to '{prefix}'",
                Context.User.Username, Context.User.Discriminator, Context.Guild.Name, prefix);
        }


        [Command("mute")]
        [Summary("mute a user")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task Mute([Summary("The user to mute")]SocketGuildUser user, 
            [Summary("Number of minutes to mute for")]int minutes=5, 
            [Summary("The reason for muting")][Remainder]string reason = null)
        {
            await Context.Channel.TriggerTypingAsync();

            if (user.Hierarchy > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendEmbedAsync("Invalid User", "That user has a higher position than the bot!",
                    await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("ERROR_IMAGES"));
                return;
            }

            // Check for muted role, attempt to create it if it doesn't exist
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if(role == null)
            {
                role = await Context.Guild.CreateRoleAsync("Muted", new GuildPermissions(sendMessages: false), null, false, null);
            }

            if(role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendEmbedAsync("Invalid permissions", "the muted role has a higher position than the bot!",
                await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("ERROR_IMAGES"));
                return;
            }

            if(user.Roles.Contains(role))
            {
                await Context.Channel.SendEmbedAsync("Already Muted", "That user is already muted!",
                    await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("ERROR_IMAGES"));
                return;
            }

            await role.ModifyAsync(x => x.Position = Context.Guild.CurrentUser.Hierarchy);
            foreach (var channel in Context.Guild.Channels)
            {
                if(!channel.GetPermissionOverwrite(role).HasValue || channel.GetPermissionOverwrite(role).Value.SendMessages == PermValue.Allow)
                {
                    await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(sendMessages: PermValue.Deny));
                }
            }

            MuteHandler.AddMute(new Mute { Guild = Context.Guild, User = user, End = DateTime.Now + TimeSpan.FromMinutes(minutes), Role = role });
            await user.AddRoleAsync(role);
            await Context.Channel.SendEmbedAsync($"Muted {user.Username}", $"Duration: {minutes} minutes\nReason: {reason ?? "None"}",
                await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("MUTE_IMAGES"));

            await _servers.SendLogsAsync(Context.Guild, "Muted", $"{Context.User.Mention} muted {user.Mention}");
            _logger.LogInformation("{user} muted {target} in {server}", Context.User.Username, user.Username, Context.Guild.Name);
        }

        [Command("unmute")]
        [Summary("unmute a user")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        private async Task Unmute([Summary("The user to unmute")]SocketGuildUser user)
        {
            var role = (Context.Guild as IGuild).Roles.FirstOrDefault(x => x.Name == "Muted");
            if (role == null)
            {
                await Context.Channel.SendEmbedAsync("Not Muted", "This person has not been muted!", await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("ERROR_IMAGES"));
                return;
            }

            if (role.Position > Context.Guild.CurrentUser.Hierarchy)
            {
                await Context.Channel.SendEmbedAsync("Invalid permissions", "the muted role has a higher position than the bot!",
                await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("ERROR_IMAGES"));
                return;
            }

            if (!user.Roles.Contains(role))
            {
                await Context.Channel.SendEmbedAsync("Not Muted", "This person has not been muted!",
                    await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("ERROR_IMAGES"));
                return;
            }

            await user.RemoveRoleAsync(role);
            await Context.Channel.SendEmbedAsync($"Unmuted {user.Username}", "Succesfully unmuted the user",
                await _servers.GetEmbedColor(Context.Guild.Id), ImageLookupUtility.GetImageUrl("UNMUTE_IMAGES"));

            await _servers.SendLogsAsync(Context.Guild, "Un-muted", $"{Context.User.Mention} unmuted {user.Mention}");
            _logger.LogInformation("{user} unmuted {target} in {server}", Context.User.Username, user.Username, Context.Guild.Name);
        }

        [Command("slowmode")]
        [RequireUserPermission(GuildPermission.ManageChannels)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        [Summary("Enable slowmode")]
        public async Task SlowMode([Summary("The number of seconds a user must wait before sending additional messages")]int interval = 0)
        {
            await Context.Channel.TriggerTypingAsync();
            await (Context.Channel as SocketTextChannel).ModifyAsync(x => x.SlowModeInterval = interval);
            await Context.Channel.SendEmbedAsync("Slowmode", $"The slowmode interval was adjusted to {interval} seconds!", await _servers.GetEmbedColor(Context.Guild.Id));

            await _servers.SendLogsAsync(Context.Guild, "Slow Mode", $"{Context.User.Mention} set slowmode interval to {interval} for {Context.Channel.Name}");

            _logger.LogInformation("{user} set slowmode to {value} in {server}", Context.User.Username, interval, Context.Guild.Name);
        }

        [Command("logs")]
        [Alias("log")]
        [Summary("Change logging settings")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Logs([Summary("Option: channel or clear")]string option = null, [Summary("Channel to log to")]string value = null)
        {
            if (option == null && value == null)
            {
                SendLoggingChannelInformation();
                return;
            }

            if (option.ToLowerInvariant() == "channel" && value != null)
            {
                SetLoggingChannelInformation(value);
                return;
            }

            if (option.ToLowerInvariant() == "clear" && value == null)
            {
                await _servers.ClearLoggingChannel(Context.Guild.Id);
                await ReplyAsync("Successfully cleared the logging channel!");
                _logger.LogInformation("{user} cleared the logging channel for {server}", Context.User.Username, Context.Guild.Name);
                return;
            }

            var server =await ServerHelper.GetOrAddServer(Context.Guild.Id, _serverRepository);
            await ReplyAsync($"You did not use this command properly!\n" +
                $"use `{server.Prefix}logs channel {{channel}}` to set the logging channel\n" +
                $"use `{server.Prefix}logs clear` to clear the logging channel.");
        }

        private async void SetLoggingChannelInformation(string value)
        {
            if (!MentionUtils.TryParseChannel(value, out ulong parserId))
            {
                await ReplyAsync("Please pass in a valid channel!");
                return;
            }

            var parsedChannel = Context.Guild.GetTextChannel(parserId);
            if (parsedChannel == null)
            {
                await ReplyAsync("Please pass in a valid channel!");
                return;
            }

            await _servers.ModifyLoggingChannel(Context.Guild.Id, parserId);
            await ReplyAsync($"Successfully modified the logging channel to {parsedChannel.Mention}");

            var perms = Context.Guild.CurrentUser.GetPermissions(parsedChannel);
            if (!perms.SendMessages)
            {
                await ReplyAsync("`Warning` the bot does not have permisson to send messages to the logging channel!");
            }

            _logger.LogInformation("{user} set the logging channel to {value} for {server}",
                Context.User.Username, value, Context.Guild.Name);
        }

        private async void SendLoggingChannelInformation()
        {
            var loggingChannelId = await _servers.GetLoggingChannel(Context.Guild.Id);
            if (loggingChannelId == 0)
            {
                await ReplyAsync("The logging channel has not yet been set!");
                return;
            }

            var loggingChannel = Context.Guild.GetTextChannel(loggingChannelId);
            if (loggingChannel == null)
            {
                await ReplyAsync("The logging channel has not yet been set!");
                await _servers.ClearLoggingChannel(Context.Guild.Id);
                return;
            }
            else
            {
                var perms = Context.Guild.CurrentUser.GetPermissions(loggingChannel);
                if(!perms.SendMessages)
                {
                    await ReplyAsync("`Warning` the bot does not have permisson to send messages to the logging channel!");
                }
                await ReplyAsync($"The logging channel is {loggingChannel.Mention}");
            }
        }

        [Command("embedcolor")]
        [Summary("Change embed color")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task EmbedColor([Summary("Comma seperated RGB value or random")][Remainder] string color = null)
        {
            await Context.Channel.TriggerTypingAsync();
            var embedColor = await _servers.GetEmbedColor(Context.Guild.Id);

            if (color == null)
            {
                string message = string.Empty;
                if (await _servers.UsingRandomEmbedColor(Context.Guild.Id))
                {
                    message = $"The embed color is `random`"; 
                }
                else
                {
                    message = $"The embed color is `{embedColor.R}, {embedColor.B}, {embedColor.G}`";
                }
                await Context.Channel.SendEmbedAsync("Embed Color", message, embedColor);
                return;
            }

            if (color.ToLowerInvariant() == "random")
            {
                await _servers.ModifyEmbedColor(Context.Guild.Id, "0,0,0");
            }
            else if (!ColorHelper.isValidColor(color))
            {
                await ReplyAsync("Unable to parse input as a color!");
                return;
            }
            else
            {
                await _servers.ModifyEmbedColor(Context.Guild.Id, color);
            }

            embedColor = await _servers.GetEmbedColor(Context.Guild.Id);
            await Context.Channel.SendEmbedAsync("Embed Color Set", $"The embed color has been modified to `{color}`", embedColor);
            await _servers.SendLogsAsync(Context.Guild, "Embed Color Changed", $"{Context.User.Mention} changed the embed color to `{color}`");
            _logger.LogInformation("{user} changed the embed color to {color} in {server}", Context.User.Username, color, Context.Guild.Name);
        }
    }
}
