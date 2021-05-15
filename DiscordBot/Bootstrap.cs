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
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.DataAccess;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;
using Victoria;

namespace DiscordBot
{
    internal static class Bootstrap
    {
        /// <summary>
        /// Setup logging outsite of DI incase we need if before DI is setup
        /// </summary>
        internal static void SetupLogging()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
            //.AddEnvironmentVariables(); // Don't think we need this at the moment, avoids installing another nuget package

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configBuilder.Build())
                .Enrich.FromLogContext()
                .CreateLogger();
            Log.Logger.Information("DiscordBot starting");

        }

        /// <summary>
        /// Setup Dependency Injection
        /// </summary>
        /// <param name="args">Command Line arguments</param>
        /// <returns>The DI Container</returns>
        internal static ServiceProvider Initialize(string[] args)
        {
            //IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
                //.AddEnvironmentVariables();
            IConfiguration config = configBuilder.Build();

            CommandService commandService = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = RunMode.Async,
                CaseSensitiveCommands = false,
            });

            DiscordSocketClient socketClient = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000,
                AlwaysDownloadUsers = true
            });

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddLogging(loggingBuilder =>
                    loggingBuilder.AddSerilog(dispose: true))
                .AddLavaNode(x =>
                {
                    x.SelfDeaf = true;
                    x.LogSeverity = LogSeverity.Info;
                    x.Authorization = "notarealpassword";
                })
                .AddSingleton<InteractiveService>()
                .AddSingleton<IServerRepository, ServerRepository>()
                .AddSingleton(config)
                .AddSingleton<LoggingService>()
                .AddSingleton(socketClient)
                .AddSingleton<IChatService, DiscordService>()
                .AddSingleton(commandService)
                .AddSingleton<CommandHandler>()
                .AddSingleton<Settings>()
                .AddSingleton<IServerService, ServerService>()
                .AddSingleton<ImageService>()
                .AddSingleton<RankService>()
                .AddSingleton<AutoRoleService>()
                .AddSingleton<IRankRepository, RankRepository>()
                .AddSingleton<IAutoRoleRepository, AutoRoleRepository>()
                .AddSingleton<ISubredditRepository, SubredditRepository>()
                .AddSingleton<IDiscordBotSettingsRepository, DiscordBotSettingsRepository>();

            var serviceProvider = serviceCollection.BuildServiceProvider();

            //TODO I'm not a big fan of this
            // We need this so the ctor gets called and the commandHandler actually gets instantiated
            // This way the events get hooked up
            // I don't think this is the place to do this, or there must be a better way
            serviceProvider.GetRequiredService<CommandHandler>();
            serviceProvider.GetRequiredService<LoggingService>();

            return serviceProvider;
        }
    }
}