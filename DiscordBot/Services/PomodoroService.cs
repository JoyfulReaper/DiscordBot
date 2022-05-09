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

using Discord.WebSocket;
using DiscordBot.Interactions.SlashCommands.General;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services;
public class PomodoroService
{
    private static List<Pomodoro> _pomodoros = new List<Pomodoro>();
    private readonly ILogger<PomodoroService> _logger;

    public PomodoroService(ILogger<PomodoroService> logger)
    {
        _logger = logger;
    }

    public void AddPomodoro(Pomodoro pomodoro)
    {
        _pomodoros.Add(pomodoro);
    }

    internal async Task PomodoroWorker(DiscordSocketClient client)
    {
        List<Pomodoro> remove = new List<Pomodoro>();

        foreach (var pomodoro in _pomodoros)
        {
            if (DateTime.Now < pomodoro.End)
            {
                continue;
            }

            remove.Add(pomodoro);
        }

        if (remove.Count > 0)
        {
            foreach (var pomo in remove)
            {
                string message =
                    $"{pomo.User.Mention}, your {pomo.TimerType} timer for {pomo.Task} has expired!";

                var dmChannel = await pomo.User.CreateDMChannelAsync();
                await dmChannel.SendMessageAsync(message);

                var channel = pomo.Channel as SocketTextChannel;
                if (pomo.Channel != null && channel != null)
                {
                    await channel.SendMessageAsync(message);
                }

                _logger.LogDebug("Pomodoro Timer Expired: {pomodoroExpired} Type: {Type}", pomo.User.Username,
                   pomo.TimerType);
            }
        }

        _pomodoros = _pomodoros.Except(remove).ToList();

        await Task.Delay(TimeSpan.FromMinutes(1));
        await PomodoroWorker(client);
    }
}