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

using Discord.Commands;
using Microsoft.Extensions.Logging;

namespace DiscordBotLibrary.Helpers;

public static class LogHelper
{
    public static void LogCommandUse(SocketCommandContext context,
        string commandName,
        ILogger logger,
        LogLevel logLevel = LogLevel.Information)
    {
        var message = "{username}#{discriminator} executed {command} on {server}/{channel}";

        switch (logLevel)
        {
            case LogLevel.Trace:
                logger.LogTrace(message, context.User.Username, context.User.Discriminator, commandName, context.Guild?.Name ?? "DM", context.Channel.Name);
                break;
            case LogLevel.Debug:
                logger.LogDebug(message, context.User.Username, context.User.Discriminator, commandName, context.Guild?.Name ?? "DM", context.Channel.Name);
                break;
            case LogLevel.Information:
                logger.LogInformation(message, context.User.Username, context.User.Discriminator, commandName, context.Guild?.Name ?? "DM", context.Channel.Name);
                break;
            case LogLevel.Warning:
                logger.LogWarning(message, context.User.Username, context.User.Discriminator, commandName, context.Guild?.Name ?? "DM", context.Channel.Name);
                break;
            case LogLevel.Error:
                logger.LogError(message, context.User.Username, context.User.Discriminator, commandName, context.Guild?.Name ?? "DM", context.Channel.Name);
                break;
            case LogLevel.Critical:
                logger.LogCritical(message, context.User.Username, context.User.Discriminator, commandName, context.Guild?.Name ?? "DM", context.Channel.Name);                
                break;
            default:
                logger.LogInformation(message, context.User.Username, context.User.Discriminator, commandName, context.Guild?.Name ?? "DM", context.Channel.Name);
                break;
        }
    }
}
