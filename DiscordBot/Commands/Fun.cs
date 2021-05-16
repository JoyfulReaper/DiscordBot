using Discord;
using Discord.Commands;
using DiscordBot.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Fun : ModuleBase<SocketCommandContext>
    {
        private static readonly Random _random = new();
        private static readonly List<string> _eightBallImages = new List<string>
        {
            "https://upload.wikimedia.org/wikipedia/commons/9/90/Magic8ball.jpg",
            "https://media1.tenor.com/images/821d79609a5bc1395d8dacab2ad8e8b6/tenor.gif?itemid=17798802",
            "https://media2.giphy.com/media/26xBJp4dcSdGxv2Zq/giphy.gif?cid=ecf05e47pnz54n4axc22ms430kzxmt8t1db6jm6qfm5vmg1p&rid=giphy.gif&ct=g"
        };

        private static readonly List<string> _eightBallResponses = new List<string>
        {
            "It is Certian.", "It is decidedly so.", "Without a doubt.", "Yes definitely.", "You may rely on it.",
            "As I see it, yes.", "Most likely.", "Outlook good.", "Yes.", "Signs point to yes.",
            "Reply hazy, try again.", "Ask again later.", "Better not tell you now.", "Cannot predict now.", "Concentrate and ask again.",
            "Don't count on it.", "My reply is no.", "My sources say no.", "Outlook not so good.", "Very doubtful."
        };

        public Fun(ILogger<Fun> logger)
        {
            
        }

        [Command("8ball")]
        [Alias("eightBall")]
        public async Task EightBall([Remainder]string question)
        {
            //var eightBallImage = await ImageHelper.SendImageAsync(Context.Channel, _eightBallImages.RandomItem());

            var builder = new EmbedBuilder();
            builder
                .WithTitle("Magic 8ball")
                .WithThumbnailUrl(_eightBallImages.RandomItem())
                .WithDescription($"{Context.User.Username} asked ***{question}***")
                .AddField("Response", _eightBallResponses.RandomItem())
                .WithColor(ColorHelper.GetColor())
                .WithCurrentTimestamp();

            await ReplyAsync(null, false, builder.Build());
            //await Task.Delay(2500);
            //await eightBallImage.DeleteAsync();
        }
    }
}
