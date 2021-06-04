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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using DiscordBot.Helpers;

namespace DiscordBot.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly ISettings _settings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommandHandler> _logger;
        private readonly IServerService _servers;
        private readonly ImageService _images;
        private readonly IConfiguration _configuration;
        private readonly IAutoRoleService _autoRoleService;

        public CommandHandler(DiscordSocketClient client,
            CommandService commands,
            ISettings settings,
            IServiceProvider serviceProvider,
            ILogger<CommandHandler> logger,
            IServerService servers,
            ImageService images,
            IConfiguration configuration,
            IAutoRoleService autoRoleService)
        {
            _client = client;
            _commands = commands;
            _settings = settings;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _servers = servers;
            _images = images;
            _configuration = configuration;
            _autoRoleService = autoRoleService;

            _client.MessageReceived += OnMessageReceived;
            _client.UserJoined += OnUserJoined;
            _client.ReactionAdded += OnReactionAdded;
            _client.MessageUpdated += OnMessageUpated;

            _commands.CommandExecuted += OnCommandExecuted;

            Task.Run(async () => await MuteHandler.MuteWorker(client));
        }

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
                await CheckForServerInvites(after as SocketUserMessage, channel.Guild);

                var server = await _servers.GetServer(channel.Guild);
                if (server != null && server.FilterProfanity)
                {
                    var badWords = ProfanityHelper.GetProfanity(after.Content);

                    if (badWords.Count != 0)
                    {
                        await ProfanityHelper.HandleProfanity(message, server, badWords);
                    }
                }
            }
        }

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
            var guild = channel?.Guild;

            if (channel != null) // Not a DM
            {
                await CheckForServerInvites(message, guild);

                var server = await _servers.GetServer(channel.Guild);
                if (server != null && server.FilterProfanity)
                {
                    var badWords = ProfanityHelper.GetProfanity(message.Content);

                    if (badWords.Count != 0)
                    {
                        await ProfanityHelper.HandleProfanity(message, server, badWords);
                    }
                }
            }

            string prefix = await _servers.GetGuildPrefix(channel.Guild.Id);

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

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedEntity, ISocketMessageChannel channel, SocketReaction reaction)
        {
            //TODO Maybe find a way to make this useful...
            //TODO Maybe keep a DB of reactable messages
            // and find a way to have them trigger something
            if (reaction.MessageId != 843516844794314782)
            {
                return;
            }

            if(reaction.Emote.Name != "✅")
            {
                return;
            }

            var textChannel = await (channel as ITextChannel).Guild.GetTextChannelAsync(821113360711155729);
            await textChannel.SendMessageAsync($"{reaction.User.Value.Mention} reacted with the ✅");
        }

        private async Task OnUserJoined(SocketGuildUser userJoining)
        {
            await AutoRoleHelper.AssignAutoRoles(_autoRoleService, userJoining);
            Task.Run(async () => await ShowWelcomeMessage(userJoining));
        }

        private async Task ShowWelcomeMessage(SocketGuildUser userJoining)
        {
            // TODO Make this a per server option

            bool showMessage = false;

            try
            {
                showMessage = bool.Parse(_configuration.GetSection("ShowWelcomeMessage").Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse ShowWelecomMessage, using {value}", showMessage);
            }

            if (showMessage)
            {
                _logger.LogInformation("Showing welcome message for {user} in {server}", userJoining.Username, userJoining.Guild.Name);

                var channelId = await _servers.GetWelcomeChannel(userJoining.Guild.Id);
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

                await channel.SendMessageAsync($"{userJoining.Username} {_settings.WelcomeMessage}");

                var background = await _servers.GetBackground(userJoining.Guild.Id);
                var memoryStream = await _images.CreateImage(userJoining, background);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                await channel.SendFileAsync(memoryStream, $"{userJoining.Username}.png");
            }
        }

        private async Task CheckForServerInvites(SocketUserMessage message, IGuild guild)
        {
            var server = await _servers.GetServer(guild);
            if (server == null || server.AllowInvites || message == null)
            {
                return;
            }

            if(message.Content.Contains("https://discord.gg/"))
            {
                if((message.Channel as SocketGuildChannel).Guild.GetUser(message.Author.Id).GuildPermissions.Administrator)
                {
                    return;
                }

                await message.DeleteAsync();
                await message.Channel.SendMessageAsync($"{message.Author.Mention} You cannot send Discord Invite links!");

                _logger.LogInformation("{user} was denied posting an invite in {server}/{channel}", message.Author.Username, guild.Name, message.Channel);
            }
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (result.Error == CommandError.UnknownCommand)
            {
                //TODO add multiple images
                //TODO make this optional/a setting
                Task.Run(async () =>
                {
                    _logger.LogDebug("{user} attempted to use an unknown command ({command}) on {server}/{channel}",
                        context.User.Username, context.Message.Content, context.Guild?.Name ?? "DM", context.Channel);

                    var badCommandMessage = await context.Channel.SendMessageAsync("https://www.wheninmanila.com/wp-content/uploads/2017/12/meme-kid-confused.png");
                    await Task.Delay(3500);
                    await badCommandMessage.DeleteAsync();
                });

                return;
            }

            if (!result.IsSuccess)
            {
                if(result.Error == CommandError.ObjectNotFound)
                {
                    await context.Channel.SendMessageAsync($"Unknown object");
                }

                if(result.Error == CommandError.UnmetPrecondition)
                {
                    // TODO better error message
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }

                _logger.LogError("Error Occured for command {command}: {error} in {server}/{channel}",
                    context.Message.Content, result.Error, context.Guild?.Name ?? "DM", context.Channel);
                Console.WriteLine($"The following error occured: {result.Error}");
            }
        }
    }
}
