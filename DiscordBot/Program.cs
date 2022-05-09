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

Some elements of this bot were insipred by the Discord.NET Bot Development Series:
https://www.youtube.com/playlist?list=PLaqoc7lYL3ZDCDT9TcP_5hEKuWQl7zudR

Although most of the code is modified from the orginal 
(For example here we use Dapper vs EF and SQLite vs MySQL)
any code used from the series is licensed under MIT as well:
https://github.com/Directoire/dnbds
*/


using DiscordBot;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

ILogger _logger = Log.ForContext<Program>();

_logger.Information("Starting Discord Bot");

IServiceProvider serviceProvider = Bootstrap.Initialize(args);
IConfiguration config = serviceProvider.GetRequiredService<IConfiguration>();

ConsoleHelper.ColorWriteLine(ConsoleColor.Red, $"{config["BotInformation:BotName"]}");
ConsoleHelper.ColorWriteLine(ConsoleColor.Blue, $"MIT License\n\nCopyright(c) 2022 Kyle Givler (JoyfulReaper)\n{config["Botinformation:BotWebsite"]}\n\n");

serviceProvider.GetRequiredService<ILoggingService>();
IDiscordService discordService = serviceProvider.GetRequiredService<IDiscordService>();

await discordService.Start();
await Task.Delay(-1);