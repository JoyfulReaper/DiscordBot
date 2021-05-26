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
using DiscordBot.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    public class Fun : ModuleBase<SocketCommandContext>
    {
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
        private readonly ILogger<Fun> _logger;

        public Fun(ILogger<Fun> logger)
        {
            _logger = logger;
        }

        [Command("8ball")]
        [Alias("eightBall")]
        [Summary("Ask the 8ball a question, get the answer!")]
        public async Task EightBall([Summary("The question to ask")][Remainder]string question)
        {
            var builder = new EmbedBuilder();
            builder
                .WithTitle("Magic 8ball")
                .WithThumbnailUrl(_eightBallImages.RandomItem())
                .WithDescription($"{Context.User.Username} asked ***{question}***")
                .AddField("Response", _eightBallResponses.RandomItem())
                .WithColor(ColorHelper.GetColor())
                .WithCurrentTimestamp();

            await ReplyAsync(null, false, builder.Build());
            _logger.LogInformation("{user} asked the 8ball {question} in {server}", Context.User.Username, question, Context.Guild.Name);
        }
    }
}
