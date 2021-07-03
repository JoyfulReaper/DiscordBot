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
using DiscordBotLib.DataAccess;
using DiscordBotLib.Helpers;
using DiscordBotLib.Models;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DiscordBot.Commands.Moderation
{
    [Name("Welcome Commands")]
    [Group("welcome")]
    public class WelcomeModule : ModuleBase<SocketCommandContext>
    {
        private readonly IServerService _servers;
        private readonly ILogger<WelcomeModule> _logger;
        private readonly IServerRepository _serverRepository;

        public WelcomeModule(IServerService servers,
            ILogger<WelcomeModule> logger,
            IServerRepository serverRepository)
        {
            _servers = servers;
            _logger = logger;
            _serverRepository = serverRepository;
        }

        [Command("setting")]
        [Summary("Enable or disable user welcoming")]
        public async Task Welcoming(string option = null)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{user}#{discriminator} invoked welcome setting ({option}) in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, option, Context.Channel.Name, Context.Guild.Name);

            var server = await ServerHelper.GetOrAddServer(Context.Guild.Id, _serverRepository);

            if(option.ToLowerInvariant() == "enable")
            {
                server.WelcomeUsers = true;
                await _serverRepository.EditAsync(server);

                await _servers.SendLogsAsync(Context.Guild, "Welcome Users", $"{Context.User.Mention} set User Welcoming `enable`");
                _logger.LogInformation("User Welcoming enabled by {user} in server {server}", Context.User.Username, Context.Guild.Name);

                await ReplyAsync("User welcoming has been enabled!");

                return;
            }
            else if (option.ToLowerInvariant() == "disable")
            {
                server.WelcomeUsers = false;
                await _serverRepository.EditAsync(server);

                await _servers.SendLogsAsync(Context.Guild, "Welcome Users", $"{Context.User.Mention} set User Welcoming `disable`");
                _logger.LogInformation("User Welcoming disabled by {user} in server {server}", Context.User.Username, Context.Guild.Name);

                await ReplyAsync("User welcoming has been disabled!");

                return;
            }

            await ReplyAsync("Would you to `enable` or `disable` user welcoming?");
            return;
        }

        [Command("")]
        [Summary("Show current settings")]
        public async Task WelcomeSettings()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{user}#{discriminator} invoked welcome in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, Context.Channel.Name, Context.Guild.Name);

            SendWelcomeChannelInformation(await _servers.GetServer(Context.Guild));
        }

        [Command("channel")]
        [Summary("Change the welcome channel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task WelcomeChannel([Summary("The channel to send welcome messages to, leave blank to clear")] SocketTextChannel channel = null)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{user}#{discriminator} invoked welcome channel ({channel}) in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, channel ,Context.Channel.Name, Context.Guild.Name);

            SetWelcomeChannelInformation(channel);
        }

        [Command("banner")]
        [Alias("image")]
        [Summary("Change the welcome banner image")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task WelcomeBanner([Summary("The channel to send welcome messages to, leave blank to clear")] string image = null)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{user}#{discriminator} invoked welcome banner ({image}) in {channel} on {server}",
                Context.User.Username, Context.User.Discriminator, image, Context.Channel.Name, Context.Guild.Name);

            SetWelcomeBannerBackgroundInformation(image);
        }

        //[Command("oldwelcome")]
        //[Summary("OLDChange user welcoming settings")]
        //[RequireUserPermission(GuildPermission.Administrator)]
        //public async Task Welcome(
        //    [Summary("Option to change: channel, background or clear")] string option = null,
        //    [Summary("Value to assign to the option")] string value = null)
        //{
            //await Context.Channel.TriggerTypingAsync();

            //var server = await _servers.GetServer(Context.Guild);
            //if (option == null && value == null)
            //{
            //    SendWelcomeChannelInformation(server);
            //    return;
            //}

            //if (option.ToLowerInvariant() == "channel" && value != null)
            //{
            //    SetWelcomeChannelInformation(value);
            //    return;
            //}

            //if (option.ToLowerInvariant() == "background" && value != null)
            //{
            //    SetWelcomeBannerBackgroundInformation(value);
            //    return;
            //}

            //if (option.ToLowerInvariant() == "clear" && value == null)
            //{
            //    await _servers.ClearWelcomeChannel(Context.Guild.Id);
            //    await ReplyAsync("Successfully cleared the welcome channel!");

            //    await _servers.SendLogsAsync(Context.Guild, "Welcome channel cleared", $"{Context.User.Mention} cleared the welcome channel");
            //    _logger.LogInformation("Welcome channel cleared by {user} in server {server}", Context.User.Username, Context.Guild.Name);
            //    return;
            //}

            //if (option.ToLowerInvariant() == "on" && value == null)
            //{
            //    server.WelcomeUsers = true;
            //    await _serverRepository.EditAsync(server);

            //    await _servers.SendLogsAsync(Context.Guild, "Welcome Users", $"{Context.User.Mention} set User Welcoming `on`");
            //    _logger.LogInformation("User Welcoming enabled by {user} in server {server}", Context.User.Username, Context.Guild.Name);

            //    await ReplyAsync("User welcoming has been enabled!");

            //    return;
            //}

            //if (option.ToLowerInvariant() == "off" && value == null)
            //{
            //    server.WelcomeUsers = false;
            //    await _serverRepository.EditAsync(server);

            //    await _servers.SendLogsAsync(Context.Guild, "Welcome Users", $"{Context.User.Mention} set User Welcoming `off`");
            //    _logger.LogInformation("User Welcoming disabled by {user} in server {server}", Context.User.Username, Context.Guild.Name);

            //    await ReplyAsync("User welcoming has been disabled!");

            //    return;
            //}

            //await ReplyAsync("You did not use this command properly!\n" +
            //    "*options:*\n" +
            //    "channel (channel): Change welcome channel\n" +
            //    "background (image url): Change welcome banner background\n" +
            //    "clear: Clear the welcome channel\n" +
            //    "on/off: Turn user welcoming `on` or `off`");
        //}

        private async void SetWelcomeChannelInformation(SocketTextChannel channel = null)
        {
            //if (!MentionUtils.TryParseChannel(value, out ulong parserId))
            //{
            //    await ReplyAsync("Please pass in a valid channel!");
            //    return;
            //}

            //var parsedChannel = Context.Guild.GetTextChannel(parserId);
            //if (parsedChannel == null)
            //{
            //    await ReplyAsync("Please pass in a valid channel!");
            //    return;
            //}

            //if (channel == null)
            //{
            //    await _servers.ClearWelcomeChannel(Context.Guild.Id);
            //    await ReplyAsync("Successfully cleared the welcome channel!");

            //    await _servers.SendLogsAsync(Context.Guild, "Welcome channel cleared", $"{Context.User.Mention} cleared the welcome channel");
            //    _logger.LogInformation("Welcome channel cleared by {user} in server {server}", Context.User.Username, Context.Guild.Name);
            //    return;
            //}

            await _servers.ModifyWelcomeChannel(Context.Guild.Id, channel.Id);
            await ReplyAsync($"Successfully modified the welcome channel to {channel.Mention}");
            await _servers.SendLogsAsync(Context.Guild, "Welcome Channel Modified", $"{Context.User} modified the welcome channel to {channel.Mention}");

            _logger.LogInformation("Welcome channel modified to {channel} by {user} in {server}",
                channel.Name, Context.User.Username, Context?.Guild.Name ?? "DM");
        }

        private async void SendWelcomeChannelInformation(Server server)
        {
            if (!server.WelcomeUsers)
            {
                await ReplyAsync("User welcoming is *`not`* enabled!");
            }
            else
            {
                await ReplyAsync("User welcoming *`is`* enabled!");
            }

            var welcomeChannelId = server.WelcomeChannel;
            if (welcomeChannelId == 0)
            {
                await ReplyAsync("The welcome channel has not yet been set!");
                //return;
            }
            else if (Context.Guild.GetTextChannel(welcomeChannelId) == null)
            {
                await ReplyAsync("The welcome channel has not yet been set!");
                await _servers.ClearWelcomeChannel(Context.Guild.Id);
                //return;
            }
            else
            {
                await ReplyAsync($"The welcome channel is {Context.Guild.GetTextChannel(welcomeChannelId).Mention}");
            }

            var welcomeBackground = await _servers.GetBackground(Context.Guild.Id);
            if (welcomeBackground != null)
            {
                await ReplyAsync($"The background is {welcomeBackground}.");
            }
            else
            {
                await ReplyAsync($"The background has not been set.");
            }
        }

        private async void SetWelcomeBannerBackgroundInformation(string value)
        {
            if (value == null)
            {
                await _servers.ClearBackground(Context.Guild.Id);
                await ReplyAsync("Successfully cleared background!");
                await _servers.SendLogsAsync(Context.Guild, "Background cleared", $"{Context.User} cleared the welcome image background.");
                return;
            }

            await _servers.ModifyWelcomeBackground(Context.Guild.Id, value);
            await _servers.SendLogsAsync(Context.Guild, "Background Modified", $"{Context.User} modified the welcome image background to {value}");
            _logger.LogInformation("Background image modified to {image} by {user} in {server}", value, Context.User, Context.Guild.Name);
            await ReplyAsync($"Successfully modified the background to {value}");
        }
    }
}
