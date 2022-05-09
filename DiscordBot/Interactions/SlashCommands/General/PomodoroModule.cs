/*
MIT License

Copyright(c) 2022 Kyle Givler
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

using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Services;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;

namespace DiscordBot.Interactions.SlashCommands.General;

[Group("pomodoro", "pomodoro commands")]
public class PomodoroModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly PomodoroService _pomodoroService;
    private readonly IGuildService _guildService;

    public PomodoroModule(PomodoroService pomodoroService,
        IGuildService guildService)
    {
        _pomodoroService = pomodoroService;
        _guildService = guildService;
    }

    [SlashCommand("start", "start the Pomodoro timer")]
    public async Task Start([Summary("time")] int length = 25, [Summary("Name")] string name = "Pomodoro")
    {
        await Context.Channel.TriggerTypingAsync();

        var pom = new Pomodoro
        {
            Guild = Context.Guild,
            User = Context.User,
            Channel = Context.Channel as SocketChannel,
            TimerType = "Pomodoro",
            Task = name,
            End = DateTime.Now + TimeSpan.FromMinutes(length)
        };

        _pomodoroService.AddPomodoro(pom);
        //await RespondAsync($"`{name}` Timer started!");
        await RespondAsync(embed: EmbedHelper.GetEmbed($"`{name}` Timer started!", "The Pomodor technique:\n1. Decide on the task to be done.\n2. Set the pomodoro timer (typically for 25 minutes)." +
            "\n3. Work on the task.\n4. End work when the timer rings and take a short break (typically 5–10 minutes).\n5. If you have finished fewer than three pomodoros, go back to Step 2 and repeat until you go through all three pomodoros.\n" +
            "6. After three pomodoros are done, take the fourth pomodoro and then take a long break (typically 20 to 30 minutes). Once the long break is finished, return to step 2",
            await _guildService.GetEmbedColorAsync(Context)));

    }

    [SlashCommand("shortbreak", "start the Pomodoro break timer")]
    public async Task Break(int length = 5)
    {
        await Context.Channel.TriggerTypingAsync();

        var pom = new Pomodoro
        {
            Guild = Context.Guild,
            User = Context.User,
            Channel = Context.Channel as SocketChannel,
            TimerType = "short break",
            Task = "your short break",
            End = DateTime.Now + TimeSpan.FromMinutes(length)
        };

        _pomodoroService.AddPomodoro(pom);
        await RespondAsync($"`Short break ({length} min)` Timer started!");
    }

    [SlashCommand("longbreak", "start the Pomodoro long break timer")]
    public async Task LongBreak(int length = 20)
    {
        await Context.Channel.TriggerTypingAsync();

        var pom = new Pomodoro
        {
            Guild = Context?.Guild as SocketGuild,
            User = Context?.User as SocketUser,
            Channel = Context.Channel as SocketChannel,
            TimerType = "long break",
            Task = "your long break",
            End = DateTime.Now + TimeSpan.FromMinutes(length)
        };

        _pomodoroService.AddPomodoro(pom);
        await RespondAsync($"`Long break ({length} min)` Timer started!");
    }
}
