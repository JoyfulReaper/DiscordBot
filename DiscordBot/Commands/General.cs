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
using DiscordBot.Helpers;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    // So it turns out that the bot needs the Presence and Server member intent in order for
    // All of the members of a channel to be "in scope"

    public class General : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<General> _logger;
        private readonly DiscordSocketClient _client;
        private readonly ISettings _settings;
        private readonly ImageService _images;
        private readonly IServerService _servers;

        public General(ILogger<General> logger,
            DiscordSocketClient client,
            ISettings settings,
            ImageService images,
            IServerService servers)
        {
            _logger = logger;
            _client = client;
            _settings = settings;
            _images = images;
            _servers = servers;
        }

        [Command("math")]
        [Alias("calculate", "calculator", "evaluate", "eval")]
        [Summary("Do math")]
        public async Task Math([Remainder] string math)
        {
            _logger.LogInformation("{username}#{discriminator} executed math: {math}", Context.User.Username, Context.User.Discriminator, math);
            var dt = new DataTable();

            var message = await ReplyAsync("https://i.pinimg.com/originals/97/a3/b9/97a3b92384b62eb04566a457f6d76f6c.gif");
            try
            {
                var result = dt.Compute(math, null);
                await ReplyAsync($"Result: `{result}`");
            }
            catch (EvaluateException)
            {
                await ReplyAsync("Unable to evaluate");
            }
            catch (SyntaxErrorException)
            {
                await ReplyAsync("Syntax error");
            }
            await Task.Delay(1500);
            await message.DeleteAsync();
        }

        [Command ("about")]
        [Summary("Information about the bot itself")]
        public async Task About()
        {
            _logger.LogInformation("{username}#{discriminator} invoked about", Context.User.Username, Context.User.Discriminator);

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(_client.CurrentUser.GetAvatarUrl() ?? _client.CurrentUser.GetDefaultAvatarUrl())
                .WithDescription("DiscordBot\nMIT License Copyright(c) 2021 JoyfulReaper\nhttps://github.com/JoyfulReaper/DiscordBot")
                .WithColor(ColorHelper.GetColor())
                .WithCurrentTimestamp();

            var embed = builder.Build();
            await ReplyAsync(null, false, embed);
        }

        [Command("owner")]
        [Summary("Retreive the server owner")]
        public async Task Owner()
        {
            _logger.LogInformation("{username}#{discriminator} invoked owner on: {server}", Context.User.Username, Context.User.Discriminator, Context.Guild.Name);

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context?.Guild?.Owner.GetAvatarUrl() ?? _client.CurrentUser.GetDefaultAvatarUrl())
                .WithDescription($"{Context?.Guild?.Owner.Username} is the owner of {Context.Guild.Name}")
                .WithColor(ColorHelper.GetColor())
                .WithCurrentTimestamp();

            var embed = builder.Build();
            await ReplyAsync(null, false, embed);
        }

        [Command("echo")]
        [Summary("Echoes a message")]
        // The remainder attribute parses until the end of a command
        public async Task Echo([Remainder] [Summary("The text to echo")] string message)
        {
            _logger.LogInformation("{username}#{discriminator} echoed: {message}", Context.User.Username, Context.User.Discriminator, message);
            await ReplyAsync(message);
        }

        [Command("ping")]
        [Summary ("Respongs with Pong!")]
        public async Task Ping()
        {
            _logger.LogInformation("{username}#{discriminator} invoked ping", Context.User.Username, Context.User.Discriminator);

            var builder = new EmbedBuilder();
            builder
                .WithThumbnailUrl(_client.CurrentUser.GetAvatarUrl() ?? _client.CurrentUser.GetDefaultAvatarUrl())
                .WithTitle("Ping Results")
                .WithDescription("Pong!")
                .AddField("Round-trip latency to the WebSocket server (ms):", _client.Latency, false)
                .WithColor(ColorHelper.GetColor())
                .WithCurrentTimestamp();

            await ReplyAsync(null, false, builder.Build());
            //await ReplyAsync($"Pong!\nRound-trip latency to the WebSocket server: {_client.Latency} ms");
        }

        [Command("info")]
        [Summary("Retervies some basic information about a user")]
        [Alias("user", "whois")]
        public async Task Info([Summary("Optional user to get info about")]SocketGuildUser mentionedUser = null)
        {
            if (mentionedUser == null)
            {
                mentionedUser = Context.User as SocketGuildUser;
            }

            _logger.LogInformation("{username}#{discriminator} invoked info on {target}", Context.User.Username, Context.User.Discriminator, mentionedUser);

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(mentionedUser.GetAvatarUrl() ?? mentionedUser.GetDefaultAvatarUrl())
                .WithDescription("User information:")
                .WithColor(ColorHelper.GetColor())
                .AddField("User ID", mentionedUser.Id, true)
                .AddField("Discriminator", mentionedUser.Discriminator, true)
                .AddField("Created at", mentionedUser.CreatedAt.ToString("MM/dd/yyyy"), true)
                .AddField("Joined at", mentionedUser.JoinedAt.Value.ToString("MM/dd/yyyy"), true)
                .AddField("Roles", string.Join(" ", mentionedUser.Roles.Select(x => x.Mention)))
                .WithCurrentTimestamp();

            var embed = builder.Build();
            await ReplyAsync(null, false, embed);
        }

        [Command("server")]
        [Summary("Retervies some basic information about a server")]
        public async Task Server()
        {
            _logger.LogInformation("{username}#{discriminator} invoked server on {target}", Context.User.Username, Context.User.Discriminator, Context.Guild.Name);

            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Server information:")
                .WithTitle($"{Context.Guild.Name} Information")
                .WithColor(ColorHelper.GetColor())
                .AddField("Created at", Context.Guild.CreatedAt.ToString("MM/dd/yyyy"), true)
                .AddField("Member count", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online users", (Context.Guild as SocketGuild).Users.Where(x => x.Status == UserStatus.Offline).Count() + " members", true)
                .WithCurrentTimestamp();

            var embed = builder.Build();
            await ReplyAsync(null, false, embed);
        }

        [Command("image", RunMode = RunMode.Async)]
        [Summary("Show the image banner thing")]
        public async Task Image(SocketGuildUser user = null)
        {
            if(user == null)
            {
                user = Context.Message.Author as SocketGuildUser;
            }

            var background = await _servers.GetBackground(user.Guild.Id);

            var memoryStream = await _images.CreateImage(user, background);
            memoryStream.Seek(0, SeekOrigin.Begin);
            await Context.Channel.SendFileAsync(memoryStream, $"{user.Username}.png");
        }
    }
}
