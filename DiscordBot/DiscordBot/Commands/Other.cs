using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Other : ModuleBase<SocketCommandContext>
    {
        [Command("shelly")]
        [Summary("A picture of the bot programmer's dog")]
        public async Task Shelly()
        {
            await ReplyAsync("Enjoy this photo of JofulReaper's dog!");
            await ReplyAsync("https://kgivler.com/images/Shelly/Shelly.jpg");
        }
    }
}
