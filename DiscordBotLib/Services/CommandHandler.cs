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
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using DiscordBotLib.Helpers;
using DiscordBotLib.DataAccess;
using DiscordBotLib.Enums;
using DiscordBotApiWrapper.Dtos;
using System.Linq;
using DiscordBotLib.Models.DatabaseEntities;

namespace DiscordBotLib.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly ISettings _settings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommandHandler> _logger;
        private readonly IServerService _servers;
        private readonly BannerImageService _bannerImageService;
        private readonly IAutoRoleService _autoRoleService;
        private readonly IProfanityRepository _profanityRepository;
        private readonly IApiService _apiService;
        private readonly IWelcomeMessageRepository _welcomeMessageRepository;
        private readonly IPartMessageRepository _partMessageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IInviteRepository _inviteRepository;
        private readonly IServerInviteRepository _serverInviteRepository;
        private readonly IServerRepository _serverRepository;

        public CommandHandler(DiscordSocketClient client,
            CommandService commands,
            ISettings settings,
            IServiceProvider serviceProvider,
            ILogger<CommandHandler> logger,
            IServerService servers,
            BannerImageService bannerImageService,
            IAutoRoleService autoRoleService,
            IProfanityRepository profanityRepository,
            IApiService apiService,
            IWelcomeMessageRepository welcomeMessageRepository,
            IPartMessageRepository partMessageRepository,
            IUserRepository userRepository,
            IInviteRepository inviteRepository,
            IServerInviteRepository serverInviteRepository,
            IServerRepository serverRepository)
        {
            _client = client;
            _commands = commands;
            _settings = settings;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _servers = servers;
            _bannerImageService = bannerImageService;
            _autoRoleService = autoRoleService;
            _profanityRepository = profanityRepository;
            _apiService = apiService;
            _welcomeMessageRepository = welcomeMessageRepository;
            _partMessageRepository = partMessageRepository;
            _userRepository = userRepository;
            _inviteRepository = inviteRepository;
            _serverInviteRepository = serverInviteRepository;
            _serverRepository = serverRepository;

            _client.MessageReceived += OnMessageReceived;
            _client.UserJoined += OnUserJoined;
            _client.ReactionAdded += OnReactionAdded;
            _client.MessageUpdated += OnMessageUpated;
            _client.UserLeft += OnUserLeft;
            _client.JoinedGuild += OnJoinedGuild;
            _client.Ready += OnReady;
            _client.InviteCreated += OnInviteCreated;

            _commands.CommandExecuted += OnCommandExecuted;

            ProfanityHelper.ProfanityRepository = profanityRepository;

            Task.Run(async () => await MuteHandler.MuteWorker(client));
            Task.Run(async () => await PomodoroHandler.PomodoroWorker(client));
        }

        private async Task OnInviteCreated(SocketInvite arg)
        {
            Task.Run(async () => await UpdateInvites());
        }

        private async Task OnReady()
        {
            Task.Run(async () => await UpdateInvites());
        }

        private async Task UpdateInvites()
        {
            foreach (var s in _client.Guilds)
            {
                var server = await ServerHelper.GetOrAddServer(s.Id, _serverRepository);

                if (server.TrackInvites)
                {
                    var dbinvites = await _serverInviteRepository.GetServerInvites(server.Id);
                    var invites = await s.GetInvitesAsync();

                    var newinvites =
                        from newi in invites
                        where !(
                            from oldi in dbinvites
                            select newi.Code
                            ).Contains(newi.Code)
                        select newi;

                    foreach (var inv in newinvites)
                    {
                        await _serverInviteRepository.AddAsync(new ServerInvite
                        {
                            Code = inv.Code,
                            ServerId = server.Id,
                            Uses = inv?.Uses ?? 0
                        });
                    }
                }
            }
        }

        private async Task OnJoinedGuild(SocketGuild arg)
        {
            await UpdateInvites();
        }

        private async Task OnUserLeft(SocketGuildUser user)
        {
            await ShowPartMessage(user);
        }

        // Message was edited
        private async Task OnMessageUpated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channelArg)
        {
            var message = after as SocketUserMessage;
            if (message == null || 
                after.Author.Username == _client.CurrentUser.Username 
                && after.Author.Discriminator == _client.CurrentUser.Discriminator)
            {
                return;
            }

            var channel = channelArg as SocketGuildChannel;

            if(channel != null) // Not a DM
            {
                var server = await _servers.GetServer(channel.Guild);
                await ServerHelper.CheckForServerInvites(after as SocketUserMessage, server);

                if (server != null && server.ProfanityFilterMode != ProfanityFilterMode.FilterOff)
                {
                    await ProfanityHelper.HandleProfanity(message, server, _apiService);
                }
            }
        }

        // Message was received
        private async Task OnMessageReceived(SocketMessage messageParam)
        {
            if (messageParam.Author.Username == _client.CurrentUser.Username
                && messageParam.Author.Discriminator == _client.CurrentUser.Discriminator)
            {
                return;
            }

            var message = messageParam as SocketUserMessage;
            if (message == null)
            {
                _logger.LogDebug("message was null");
                return;
            }

            var channel = message.Channel as SocketGuildChannel;
            var prefix = String.Empty;

            if (channel != null) // Not a DM
            {
                prefix = await _servers.GetGuildPrefix(channel.Guild.Id);
                if(prefix == null)
                {
                    prefix = _settings.DefaultPrefix;
                }
                var server = await _servers.GetServer(channel.Guild);
                await ServerHelper.CheckForServerInvites(message, server);
                
                if (server != null && server.ProfanityFilterMode != ProfanityFilterMode.FilterOff)
                {
                    await ProfanityHelper.HandleProfanity(message, server, _apiService);
                }
            }

            var context = new SocketCommandContext(_client, message);
            int position = 0;

            if (message.HasStringPrefix(prefix, ref position)
                || message.HasMentionPrefix(_client.CurrentUser, ref position))
            {
                _logger.LogInformation("Command received: {command}", message.Content);

                if (message.Author.IsBot)
                {
                    _logger.LogDebug("Command ({command}) was sent by another bot, ignoring", message.Content);
                    return;
                }

                await _commands.ExecuteAsync(context, position, _serviceProvider);
            }
        }

        // Reaction was added
        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedEntity, ISocketMessageChannel channel, SocketReaction reaction)
        {
            await RPSHelper.RPSProcessor(cachedEntity, channel, reaction);
        }

        // User joined a guild
        private async Task OnUserJoined(SocketGuildUser userJoining)
        {
            await AutoRoleHelper.AssignAutoRoles(_autoRoleService, userJoining);
            Task.Run(async () => await ShowWelcomeMessage(userJoining));

            var guild = userJoining.Guild;
            var server = await ServerHelper.GetOrAddServer(guild.Id, _serverRepository);
            var dbinvites = await _serverInviteRepository.GetServerInvites(server.Id);
            var invites = await guild.GetInvitesAsync();

            if (server.TrackInvites)
            {
                var inviteUsed =
                    from ginvite in invites
                    join invite in dbinvites
                    on ginvite.Code equals invite.Code
                    where invite.Uses < ginvite.Uses
                    select ginvite;

                if (inviteUsed.Count() > 1)
                {
                    _logger.LogWarning("More than one invite matches!");
                    await _servers.SendLogsAsync(userJoining.Guild, "Invite Error!", "More than one invite matched!");
                    return;
                }
                if (inviteUsed.Count() < 1)
                {
                    _logger.LogWarning("No invite matches!");
                    await _servers.SendLogsAsync(userJoining.Guild, "Invite Error!", "Could not find matching invite!");
                    return;
                }

                var invused = inviteUsed.FirstOrDefault();

                var inviter = await _userRepository.GetByUserId(invused.Inviter.Id);
                var userInvite = await _inviteRepository.GetInviteByUser(inviter.Id);

                if(userInvite == null)
                {
                    userInvite = new Invite
                    {
                        Count = 0,
                        UserId = inviter.Id
                    };
                    await _inviteRepository.AddAsync(userInvite);
                }

                userInvite.Count++;
                await _inviteRepository.EditAsync(userInvite);

                var updateInv = dbinvites.Where(x => x.Code == invused.Code).FirstOrDefault();
                updateInv.Uses = invused?.Uses ?? 0;
                await _serverInviteRepository.EditAsync(updateInv);

                //ServerInvite inviteUsed = null;
                //var guildInvites = invites.ToList();
                //for(int i = 0; i < dbinvites.Count; i++)
                //{
                //    for(int j = 0; j < invites.Count; j++)
                //    {
                //        if(dbinvites[i].Code == guildInvites[j].Code && dbinvites[i].Uses < guildInvites[j].Uses)
                //        {
                //            inviteUsed = dbinvites[i];
                //        }
                //    }
                //}

                await UpdateInvites();
            }
        }

        // Show the welcome message
        private async Task ShowWelcomeMessage(SocketGuildUser userJoining)
        {
            var server = await _servers.GetServer(userJoining.Guild);
            if (server.WelcomeUsers)
            {
                _logger.LogInformation("Showing welcome message for {user} in {server}", userJoining.Username, userJoining.Guild.Name);

                var channelId = server.WelcomeChannel;
                if(channelId == 0)
                {
                    return;
                }

                ISocketMessageChannel channel = userJoining.Guild.GetTextChannel(channelId);
                if(channel == null)
                {
                    await _servers.ClearWelcomeChannel(userJoining.Guild.Id);
                    return;
                }

                var welcomeMessages = await _welcomeMessageRepository.GetWelcomeMessagesByServerId(userJoining.Guild.Id);
                if (welcomeMessages.Count < 1)
                {
                    await channel.SendMessageAsync($"{userJoining.Mention} {_settings.WelcomeMessage}");
                }
                else
                {
                    var message = welcomeMessages.RandomItem().Message;
                    message = message.Replace("{username}", userJoining.Username);
                    message = message.Replace("{mention}", userJoining.Mention);

                    await channel.SendMessageAsync(message);
                }

                var background = await _servers.GetBackground(userJoining.Guild.Id);
                var memoryStream = await _bannerImageService.CreateImage(userJoining, background);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                await channel.SendFileAsync(memoryStream, $"{userJoining.Username}.png");
            }
        }

        private async Task ShowPartMessage(SocketGuildUser userParting)
        {
            var server = await _servers.GetServer(userParting.Guild);
            if (server.WelcomeUsers)
            {
                _logger.LogInformation("Showing parting message for {user} in {server}", userParting.Username, userParting.Guild.Name);

                var channelId = server.WelcomeChannel;
                if (channelId == 0)
                {
                    return;
                }

                ISocketMessageChannel channel = userParting.Guild.GetTextChannel(channelId);
                if (channel == null)
                {
                    await _servers.ClearWelcomeChannel(userParting.Guild.Id);
                    return;
                }

                var partMessages = await _partMessageRepository.GetPartMessagesByServerId(userParting.Guild.Id);
                if (partMessages.Count < 1)
                {
                    await channel.SendMessageAsync($"{userParting.Username} {_settings.PartingMessage}");
                }
                else
                {
                    var message = partMessages.RandomItem().Message;
                    message = message.Replace("{username}", userParting.Username);
                    message = message.Replace("{mention}", userParting.Username); //You can't mention a used that left!!

                    await channel.SendMessageAsync(message);
                }

                
            }
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (_apiService.ApiIsEnabled)
            {
                if (command.IsSpecified)
                {
                    var commandItem = new CommandItemCreateDto
                    {
                        Guild = new GuildCreateDto { GuildId = context.Guild?.Id ?? 0, GuildName = context.Guild?.Name ?? "DM" },
                        Channel = new ChannelCreateDto { ChannelId = context.Channel.Id, ChannelName = context.Channel.Name },
                        Name = command.Value.Name,
                        Module = command.Value.Module.Name,
                        Message = context.Message.Content,
                        Succesful = result.IsSuccess,
                        Date = DateTimeOffset.UtcNow,
                        User = new UserCreateDto { UserId = context.User.Id, UserName = context.User.Username }
                    };

                    try
                    {
                        await _apiService.CommandItemApi.SaveCommandItem(commandItem);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "An exception occured while trying to contact the API");
                    }
                }

            }

            if (result.Error == CommandError.UnknownCommand)
            {
                //TODO make this optional/a setting
                Task.Run(async () =>
                {
                    _logger.LogDebug("{user} attempted to use an unknown command ({command}) on {server}/{channel}",
                        context.User.Username, context.Message.Content, context.Guild?.Name ?? "DM", context.Channel);

                    var badCommandMessage = await context.Channel.SendMessageAsync(ImageLookupUtility.GetImageUrl("BADCOMMAND_IMAGES"));
                    await Task.Delay(3500);
                    await badCommandMessage.DeleteAsync();
                });

                return;
            }

            if (!result.IsSuccess)
            {
                _logger.LogError("Error Occured for command {command}: {error} in {server}/{channel}",
                    context.Message.Content, result.Error, context.Guild?.Name ?? "DM", context.Channel);

                Console.WriteLine($"The following error occured: {result.Error}");

                await context.Channel.SendMessageAsync($"The following error occured: {result.ErrorReason}");
            }
        }
    }
}
