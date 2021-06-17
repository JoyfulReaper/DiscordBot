// Just a demo of the Discord.Addons.Interactive nuget package

using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using DiscordBotLib.Helpers;
using DiscordBotLib.Services;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [Name("PaginationDemoHidden")]
    public class PaginationDemo : InteractiveBase
    {
        private readonly ISettings _settings;
        private readonly IServerService _servers;

        public PaginationDemo(ISettings settings, IServerService servers)
        {
            _settings = settings;
            _servers = servers;
        }

        // DeleteAfterAsync will send a message and asynchronously delete it after the timeout has popped
        // This method will not block.
        [Command("delete")]
        public async Task<RuntimeResult> Test_DeleteAfterAsync()
        {
            await ReplyAndDeleteAsync("this message will self-destruct in 10 seconds", timeout: TimeSpan.FromSeconds(10));
            return Ok();
        }

        // NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        // By default, this will be limited to messages from the source user in the source channel
        // This method will block the gateway, so it should be ran in async mode.
        [Command("next", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync();
            if (response != null)
            {
                if (response.Content == "4")
                {
                    await ReplyAsync("You must be some sort of genius!");
                }
                else
                {
                    await ReplyAsync($"I'm not so sure {response.Content} is the correct answer...");
                }
            }
            else
                await ReplyAsync("You did not reply before the timeout");
        }

        // PagedReplyAsync will send a paginated message to the channel
        // You can customize the paginator by creating a PaginatedMessage object
        // You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.
        [Command("paginator")]
        public async Task Test_Paginator()
        {
            var pages = new[] { "**Help**\n\n`!help` - Show the help command",
                "**Help**\n\n`!prefix` - View or change the prefix",
                "**Help**\n\n`!ping` - View the current latency"
                };

            PaginatedMessage paginatedMessage = new PaginatedMessage()
            {
                Pages = pages,
                Options = new PaginatedAppearanceOptions()
                {
                    InformationText = "THIS IS A TEST!!",
                    Info = new Emoji("❓")
                },
                Color = await _servers.GetEmbedColor(Context.Guild.Id),
                Title = "Awesome Paginator"
            };


            await PagedReplyAsync(paginatedMessage);
        }
    }
}
