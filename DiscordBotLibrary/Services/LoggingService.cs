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

using Discord;
using Discord.Commands;
using Discord.Net;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiscordBotLibrary.Services;

public class LoggingService : ILoggingService
{
    public event EventHandler? OnBadToken;

    private readonly ILogger<LoggingService> _logger;


    public LoggingService(ILogger<LoggingService> logger)
    {
        _logger = logger;
    }

    public async Task LogAsync(LogMessage message)
    {
        if (message.Exception is CommandException commandException)
        {
            _logger.LogError("[Command] {command} failed to execute in {server}:{channel}.",
                   commandException.Command.Aliases.First(), commandException.Context.Guild?.Name ?? "DM", commandException.Context.Channel.Name);
            _logger.LogError(commandException, "Exception");

            await commandException.Context.Channel.SendMessageAsync("Ooops... The bot just blew up! Command failed :(");
        }
        else
        {
            var logMessage = "[{source}] {message}";

            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(logMessage, message.Source, message.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(logMessage, message.Source, message.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(logMessage, message.Source, message.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(logMessage, message.Source, message.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogInformation(logMessage, message.Source, message.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(logMessage, message.Source, message.Message);
                    break;
                default:
                    throw new Exception("Unknown log level.");
            }
        }
        if (message.Exception != null)
        {
            _logger.LogDebug(message.Exception, "Exception:");
            if (message.Exception.InnerException != null &&
                message.Exception.InnerException is WebSocketClosedException
                && message.Exception.InnerException.Message.Contains("4004"))
            {
                OnBadToken?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
