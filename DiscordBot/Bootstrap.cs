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
using Discord.Addons.Hosting;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.DataAccess;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using Victoria;

namespace DiscordBot
{
    internal static class Bootstrap
    {
        internal static IHost Initialize(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
               {
                   var configuration = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json", false, true)
                       .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                       .Build();

                   x.AddConfiguration(configuration);
               })
                .UseSerilog()
                .ConfigureDiscordHost<DiscordSocketClient>((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 1000,
                        ExclusiveBulkDelete = true,
                    };

                    config.Token = GetToken();
                })
                .UseCommandService((context, config) =>
                {
                    config.CaseSensitiveCommands = false;
                    config.LogLevel = LogSeverity.Verbose;
                    config.DefaultRunMode = RunMode.Async;
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddHostedService<CommandHandler>()
                        .AddHostedService<LoggingService>()
                        .AddHostedService<DiscordService>()
                        .AddLavaNode(x =>
                        {
                            x.SelfDeaf = true;
                            x.LogSeverity = LogSeverity.Info;
                            x.Authorization = "notarealpassword";
                            x.LogSeverity = LogSeverity.Verbose;
                            x.UserAgent = "DiscordBot by Joyful";
                        })
                        .AddSingleton<InteractiveService>()
                        .AddSingleton<IServerRepository, ServerRepository>()
                        .AddSingleton<LoggingService>()
                        .AddSingleton<IChatService, DiscordService>()
                        .AddSingleton<CommandHandler>()
                        .AddSingleton<ISettings, Settings>()
                        .AddSingleton<IServerService, ServerService>()
                        .AddSingleton<ImageService>()
                        .AddSingleton<IRankService, RankService>()
                        .AddSingleton<IAutoRoleService, AutoRoleService>()
                        .AddSingleton<IRankRepository, RankRepository>()
                        .AddSingleton<IAutoRoleRepository, AutoRoleRepository>()
                        .AddSingleton<ISubredditRepository, SubredditRepository>()
                        .AddSingleton<IDiscordBotSettingsRepository, DiscordBotSettingsRepository>();
                })
                .UseConsoleLifetime();

            return builder.Build();
        }

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
            Log.Logger.Information("Initial Logging setup");
        }

        private static string GetToken()
        {
            return "TOKENHERE";
        }



        ///// <summary>
        ///// Setup Dependency Injection
        ///// </summary>
        ///// <param name="args">Command Line arguments</param>
        ///// <returns>The DI Container</returns>
        //internal static ServiceProvider Initialize(string[] args)
        //{
        //    //IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

        //    IConfigurationBuilder configBuilder = new ConfigurationBuilder()
        //        .SetBasePath(Directory.GetCurrentDirectory())
        //        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
        //        //.AddEnvironmentVariables();
        //    IConfiguration config = configBuilder.Build();

        //    CommandService commandService = new CommandService(new CommandServiceConfig
        //    {
        //        LogLevel = LogSeverity.Verbose,
        //        DefaultRunMode = RunMode.Async,
        //        CaseSensitiveCommands = false,
        //    });

        //    DiscordSocketClient socketClient = new DiscordSocketClient(new DiscordSocketConfig
        //    {
        //        LogLevel = LogSeverity.Verbose,
        //        MessageCacheSize = 1000,
        //        AlwaysDownloadUsers = true,
        //        ExclusiveBulkDelete = true,
        //    });

        //    var serviceCollection = new ServiceCollection();
        //    serviceCollection
        //        .AddLogging(loggingBuilder =>
        //            loggingBuilder.AddSerilog(dispose: true))
        //        .AddLavaNode(x =>
        //        {
        //            x.SelfDeaf = true;
        //            x.LogSeverity = LogSeverity.Info;
        //            x.Authorization = "notarealpassword";
        //            x.LogSeverity = LogSeverity.Verbose;
        //            x.UserAgent = "DiscordBot by Joyful";
        //        })
        //        .AddSingleton<InteractiveService>()
        //        .AddSingleton<IServerRepository, ServerRepository>()
        //        .AddSingleton(config)
        //        .AddSingleton<LoggingService>()
        //        .AddSingleton(socketClient)
        //        .AddSingleton<IChatService, DiscordService>()
        //        .AddSingleton(commandService)
        //        .AddSingleton<CommandHandler>()
        //        .AddSingleton<ISettings, Settings>()
        //        .AddSingleton<IServerService, ServerService>()
        //        .AddSingleton<ImageService>()
        //        .AddSingleton<IRankService, RankService>()
        //        .AddSingleton<IAutoRoleService, AutoRoleService>()
        //        .AddSingleton<IRankRepository, RankRepository>()
        //        .AddSingleton<IAutoRoleRepository, AutoRoleRepository>()
        //        .AddSingleton<ISubredditRepository, SubredditRepository>()
        //        .AddSingleton<IDiscordBotSettingsRepository, DiscordBotSettingsRepository>();

        //    var serviceProvider = serviceCollection.BuildServiceProvider();

        //    //TODO I'm not a big fan of this
        //    // We need this so the ctor gets called and the commandHandler actually gets instantiated
        //    // This way the events get hooked up
        //    // I don't think this is the place to do this, or there must be a better way
        //    serviceProvider.GetRequiredService<CommandHandler>();
        //    serviceProvider.GetRequiredService<LoggingService>();

        //    return serviceProvider;
        //}
    }
}