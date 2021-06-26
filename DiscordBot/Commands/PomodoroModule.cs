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

using Discord.Commands;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Commands
{
    [Name("Pomodoro")]
    [Group("pomodoro")]
    [Alias("pomo")]
    public class PomodoroModule : ModuleBase<CommandContext>
    {
        private readonly ILogger<PomodoroModule> _logger;
        private readonly IUserService _userService;

        public PomodoroModule(ILogger<PomodoroModule> logger,
            IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [Command("start")]
        [Summary("start the Pomodoro timer")]
        public async Task Start([Summary("length of timer")] int length = 25)
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed pomodoro start (Length: {length}) on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, length, Context.Guild?.Name ?? "DM", Context.Channel.Name);
        }

        [Command("shortbreak")]
        [Alias("sbreak", "break")]
        [Summary("start the Pomodoro break timer")]
        public async Task Break()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed pomodoro shortbreak on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);
        }

        [Command("longbreak")]
        [Alias("lbreak")]
        [Summary("start the Pomodoro long break timer")]
        public async Task LongBreak()
        {
            await Context.Channel.TriggerTypingAsync();

            _logger.LogInformation("{username}#{discriminator} executed pomodoro longbreak on {server}/{channel}",
                Context.User.Username, Context.User.Discriminator, Context.Guild?.Name ?? "DM", Context.Channel.Name);
        }
    }
}
