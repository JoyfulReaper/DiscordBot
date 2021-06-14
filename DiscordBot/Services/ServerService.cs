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
using DiscordBot.DataAccess;
using DiscordBot.Helpers;
using DiscordBot.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class ServerService : IServerService
    {
        private readonly Random _random = new();
        private readonly IServerRepository _serverRepository;
        private readonly ISettings _settings;
        private readonly ILogger<ServerService> _logger;

        public ServerService(IServerRepository serverRepository,
            ISettings settings,
            ILogger<ServerService> logger)
        {
            _serverRepository = serverRepository;
            _settings = settings;
            _logger = logger;
        }

        public async Task<Server> GetServer(IGuild guild)
        {
            if(guild == null)
            {
                return null;
            }

            return await _serverRepository.GetByServerId(guild.Id);
        }

        public async Task UpdateServer(Server server)
        {
            if(server == null)
            {
                return;
            }

            await _serverRepository.EditAsync(server);
        }

        public async Task<bool> UsingRandomEmbedColor(ulong serverId)
        {
            var server = await _serverRepository.GetByServerId(serverId);
            if (server.EmbedColor.RawValue != 0)
            {
                return false;
            }

            return true;
        }

        public async Task<Color> GetEmbedColor(ulong id)
        {
            var server = await _serverRepository.GetByServerId(id);
            if (server.EmbedColor.RawValue == 0)
            {
                return new Color(_random.Next(256), _random.Next(256), _random.Next(256));
            }
            else
            {
                return server.EmbedColor;
            }
        }

        public async Task ModifyEmbedColor(ulong id, string color)
        {
            var rgb = color.Split(",");
            if(rgb.Length != 3)
            {
                throw new ArgumentException("Color must be in comma seperated RBG format", nameof(color));
            }

            Color discordColor;
            try
            {
                discordColor = new Color(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
            }
            catch (Exception e)
            {
                throw new ArgumentException("Unable to parse color", nameof(color), e);
            }

            var server = await _serverRepository.GetByServerId(id);
            if (server == null)
            {
                await _serverRepository.AddAsync(new Server { GuildId = id, EmbedColor = discordColor });
            }
            else
            {
                server.EmbedColor = discordColor;
                await _serverRepository.EditAsync(server);
            }
        }

        /// <summary>
        /// Modify the prefix the bot responds to
        /// </summary>
        /// <param name="id">The server/guild id</param>
        /// <param name="prefix">The prefix to use</param>
        /// <returns></returns>
        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _serverRepository.GetByServerId(id);

            if (server == null)
            {
                await _serverRepository.AddAsync(new Server { GuildId = id, Prefix = prefix });
            }
            else
            {
                server.Prefix = prefix;
                await _serverRepository.EditAsync(server);
            }
        }

        /// <summary>
        /// Get the prefix the bot responds to for the given guild
        /// </summary>
        /// <param name="id">The id of the server/guild</param>
        /// <returns>The prefix the bot will respond to</returns>
        public async Task<string> GetGuildPrefix(ulong id)
        {
            string prefix;
            var server = await _serverRepository.GetByServerId(id);

            if (server == null)
            {
                prefix = _settings.DefaultPrefix;
                server = new Server { GuildId = id, Prefix = prefix };
                await _serverRepository.AddAsync(server);
            }
            else
            {
                prefix = server.Prefix;
            }

            return prefix;
        }

        /// <summary>
        /// Modify the channel to send welcome messages to
        /// </summary>
        /// <param name="id">The id of the server/guild</param>
        /// <param name="channelId">The id of the welcome channel</param>
        /// <returns></returns>
        public async Task ModifyWelcomeChannel(ulong id, ulong channelId)
        {
            var server = await _serverRepository.GetByServerId(id);

            if (server == null)
            {
                await _serverRepository.AddAsync(id);
                server = await _serverRepository.GetByServerId(id);

                if (server == null)
                {
                    throw new Exception("We tried to add the server but its not there!");
                }

                server.WelcomeChannel = channelId;
                await _serverRepository.EditAsync(server);
            }
            else
            {
                server.WelcomeChannel = channelId;
                await _serverRepository.EditAsync(server);
            }
        }

        /// <summary>
        /// Clear the welcome channel
        /// </summary>
        /// <param name="id">The id of the server/guild to clear the welcome channel for</param>
        /// <returns></returns>
        public async Task ClearWelcomeChannel(ulong id)
        {
            var server = await _serverRepository.GetByServerId(id);
            if (server == null)
            {
                _logger.LogDebug("Tried to clear welcome channel for server not yet in the DB");
                return;
            }

            server.WelcomeChannel = 0;
            await _serverRepository.EditAsync(server);
        }

        /// <summary>
        /// Get the welcome channel for a server
        /// </summary>
        /// <param name="id">The id of the server/guild</param>
        /// <returns>The id of the welcome channel</returns>
        public async Task<ulong> GetWelcomeChannel(ulong id)
        {
            var server = await _serverRepository.GetByServerId(id);

            return await Task.FromResult(server.WelcomeChannel);
        }

        /// <summary>
        /// Modify the background image for the welcome banner
        /// </summary>
        /// <param name="id">The is of the server/guild</param>
        /// <param name="url">The url of the background image</param>
        /// <returns></returns>
        public async Task ModifyWelcomeBackground(ulong id, string url)
        {
            var server = await _serverRepository.GetByServerId(id);

            if (server == null)
            {
                await _serverRepository.AddAsync(id);
                server = await _serverRepository.GetByServerId(id);

                if (server == null)
                {
                    throw new Exception("We tried to add the server but its not there!");
                }

                server.WelcomeBackground = url;
                await _serverRepository.EditAsync(server);
            }
            else
            {
                server.WelcomeBackground = url;
                await _serverRepository.EditAsync(server);
            }
        }

        /// <summary>
        /// Clear the background image for the welcome banner
        /// </summary>
        /// <param name="id">The id of the server/guild</param>
        /// <returns></returns>
        public async Task ClearBackground(ulong id)
        {
            var server = await _serverRepository.GetByServerId(id);
            if (server == null)
            {
                _logger.LogDebug("Tried to clear welcome background for server not yet in the DB");
                return;
            }

            server.WelcomeBackground = null;
            await _serverRepository.EditAsync(server);
        }

        /// <summary>
        /// Get the background image for the welcome banner
        /// </summary>
        /// <param name="id">Id of the server/guild</param>
        /// <returns>URL of the background image</returns>
        public async Task<string> GetBackground(ulong id)
        {
            var server = await _serverRepository.GetByServerId(id);

            return await Task.FromResult(server.WelcomeBackground);
        }

        /// <summary>
        /// Modify the logging to send logging messages to
        /// </summary>
        /// <param name="id">The id of the server/guild</param>
        /// <param name="channelId">The id of the logging channel</param>
        /// <returns></returns>
        public async Task ModifyLoggingChannel(ulong id, ulong channelId)
        {
            var server = await _serverRepository.GetByServerId(id);

            if (server == null)
            {
                await _serverRepository.AddAsync(id);
                server = await _serverRepository.GetByServerId(id);

                if (server == null)
                {
                    throw new Exception("We tried to add the server but its not there!");
                }

                server.LoggingChannel = channelId;
                await _serverRepository.EditAsync(server);
            }
            else
            {
                server.LoggingChannel = channelId;
                await _serverRepository.EditAsync(server);
            }
        }

        /// <summary>
        /// Clear the logging channel
        /// </summary>
        /// <param name="id">The id of the server/guild to clear the logging channel for</param>
        /// <returns></returns>
        public async Task ClearLoggingChannel(ulong id)
        {
            var server = await _serverRepository.GetByServerId(id);
            if (server == null)
            {
                _logger.LogDebug("Tried to clear welcome channel for server not yet in the DB");
                return;
            }

            server.LoggingChannel = 0;
            await _serverRepository.EditAsync(server);
        }

        /// <summary>
        /// Get the logging channel for a server
        /// </summary>
        /// <param name="id">The id of the server/guild</param>
        /// <returns>The id of the logging channel</returns>
        public async Task<ulong> GetLoggingChannel(ulong id)
        {
            var server = await _serverRepository.GetByServerId(id);
            if(server == null)
            {
                await _serverRepository.AddAsync(new Server { GuildId = id });
                return 0;
            }

            return await Task.FromResult(server.LoggingChannel);
        }

        public async Task SendLogsAsync(IGuild guild, string title, string description, string thumbnailUrl = null)
        {
            if(guild == null)
            {
                return;
            }

            if(thumbnailUrl == null)
            {
                thumbnailUrl = ImageLookupUtility.GetImageUrl("LOGGING_IMAGES");
            }

            var channelId = await GetLoggingChannel(guild.Id);
            if (channelId == 0)
            {
                return;
            }

            var channel = await guild.GetTextChannelAsync(channelId);
            if (channel == null)
            {
                await ClearLoggingChannel(guild.Id);
                return;
            }

            await channel.SendLogAsync(title, description, await GetEmbedColor(guild.Id), thumbnailUrl);

            // TODO: Send this to the API
            ServerLogItem serverLogItem = new ServerLogItem
            {
                GuildId = guild.Id,
                GuildName = guild.Name,
                ChannelId = channelId,
                ChannelName = channel.Name,
                Title = title,
                Description = description,
                ThumbnailUrl = thumbnailUrl
            };
        }
    }
}
