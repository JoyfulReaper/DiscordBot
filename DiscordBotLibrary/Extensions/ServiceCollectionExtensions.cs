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
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotLibrary.Services;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DiscordBotLibrary.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordBot(this IServiceCollection services,
        DiscordSocketConfig? discordSocketConfig = null,
        InteractionServiceConfig? interactionServiceConfig = null,
        CommandServiceConfig? commandServiceConfig = null)
    {
        IConfigurationBuilder configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddUserSecrets<DiscordService>();
        IConfiguration config = configBuilder.Build();

        SetupLogging(config);

        ConfigureDiscordDotNetServices(ref discordSocketConfig, 
            ref commandServiceConfig, 
            ref interactionServiceConfig);

        DiscordSocketClient socketClient = new DiscordSocketClient(discordSocketConfig);
        CommandService commandService = new CommandService(commandServiceConfig);
        InteractionService interactionService = new InteractionService(socketClient, interactionServiceConfig);

        services.AddSingleton(config);
        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));

        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton(socketClient);
        services.AddSingleton(commandService);
        services.AddSingleton(interactionService);
        services.AddSingleton<IDiscordService, DiscordService>();
        services.AddSingleton<ICommandHandler, CommandHandler>();
        services.AddSingleton<IInteractionHandler, InteractionHandler>();

        return services;
    }

    private static void ConfigureDiscordDotNetServices(ref DiscordSocketConfig? discordSocketConfig,
        ref CommandServiceConfig? commandServiceConfig,
        ref InteractionServiceConfig interactionServiceConfig)
    {
        if(interactionServiceConfig == null)
        {
            interactionServiceConfig = new InteractionServiceConfig
            {
                DefaultRunMode = Discord.Interactions.RunMode.Async,
                LogLevel = LogSeverity.Verbose,
            };
        }

        if (discordSocketConfig == null)
        {
            discordSocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 500,
                AlwaysDownloadUsers = true,
                GatewayIntents = GatewayIntents.AllUnprivileged,
            };
        }

        if (commandServiceConfig == null)
        {
            commandServiceConfig = new CommandServiceConfig
            {
                LogLevel = LogSeverity.Verbose,
                DefaultRunMode = Discord.Commands.RunMode.Async,
                CaseSensitiveCommands = false,
            };
        }
    }

    private static void SetupLogging(IConfiguration config)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .Enrich.FromLogContext()
            .CreateLogger();
        
        Log.Logger.Information("DiscordBotLibrary: Logging configured");
    }
}
