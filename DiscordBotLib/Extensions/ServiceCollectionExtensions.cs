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
using Discord.WebSocket;
using DiscordBotApiWrapper;
using DiscordBotLib.DataAccess;
using DiscordBotLib.DataAccess.Repositories;
using DiscordBotLib.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace DiscordBotLib.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDiscordBotLib(this IServiceCollection services, 
            CommandService commandService,
            DiscordSocketClient socketClient,
            ApiClient apiClient,
            IConfiguration config)
        {
                 services.AddSingleton<IApiService, ApiService>()
                .AddSingleton(config)
                .AddSingleton<ISettings, Settings>()
                .AddSingleton<LoggingService>()
                .AddSingleton<BannerImageService>()
                .AddSingleton<IServerLogItemApi, ServerLogItemApi>()
                .AddSingleton<ICommandItemApi, CommandItemApi>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<IServerRepository, ServerRepository>()
                .AddSingleton<IRankService, RankService>()
                .AddSingleton<IAutoRoleService, AutoRoleService>()
                // Start Repos
                .AddSingleton<IRankRepository, RankRepository>()
                .AddSingleton<IAutoRoleRepository, AutoRoleRepository>()
                .AddSingleton<ISubredditRepository, SubredditRepository>()
                .AddSingleton<IDiscordBotSettingsRepository, DiscordBotSettingsRepository>()
                .AddSingleton<IServerService, ServerService>()
                .AddSingleton<IUserTimeZonesRepository, UserTimeZoneRepository>()
                .AddSingleton<IProfanityRepository, ProfanityRepository>()
                .AddSingleton<IUserRepository, UserRepository>()
                .AddSingleton<INoteRepository, NoteRepository>()
                .AddSingleton<IWarningRepository, WarningRepository>()
                .AddSingleton<IWelcomeMessageRepository, WelcomeMessageRepository>()
                .AddSingleton<IPartMessageRepository, PartMessageRepository>()
                .AddSingleton<IInviteRepository, InviteRepository>()
                .AddSingleton<IServerInviteRepository, ServerInviteRepository>()
                // End Repos
                .AddSingleton<CommandHandler>()
                .AddSingleton(commandService)
                .AddSingleton(apiClient)
                .AddSingleton<IChatService, DiscordService>()
                .AddSingleton(socketClient);

            return services;
        }
    }
}
