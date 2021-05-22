using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace DiscordBot.Helpers
{
    public static class EmbedHelper
    {
        public static async Task<IMessage> SendEmbedAsync(this ISocketMessageChannel channel, string title, string description, string thumbImage = null)
        {
            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(ColorHelper.GetColor())
                .WithCurrentTimestamp();

            if(thumbImage !=null)
            {
                embed.WithThumbnailUrl(thumbImage);
            }

            var message = await channel.SendMessageAsync(embed: embed.Build());
            return message;
        }
    }
}
