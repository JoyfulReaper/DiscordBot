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
using Discord.WebSocket;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot
{
    internal static class Bootstrap
    {
        internal static ServiceProvider Initialize(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();
            CommandService commandService = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
            });

            DiscordSocketClient socketClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            });

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton(Configuration)
                .AddSingleton(socketClient)
                .AddSingleton<IChatService, DiscordService>()
                .AddSingleton(commandService)
                .AddSingleton<CommandHandler>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            //TODO I hate this... Fix it
            // We need this so the ctor gets called that the commandHandler get instantiated
            // I really think is a horrible place to do this..
            serviceProvider.GetRequiredService<CommandHandler>();

            return serviceProvider;
        }
    }
}