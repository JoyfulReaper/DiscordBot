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

using DiscordBot.DataAccess;
using DiscordBot.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class ServerService : IServerService
    {
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

        public async Task ClearWelcome(ulong id)
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

        public async Task<ulong> GetWelcome(ulong id)
        {
            var server = await _serverRepository.GetByServerId(id);

            return await Task.FromResult(server.WelcomeChannel);
        }

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

        public async Task<string> GetBackground(ulong id)
        {
            var server = await _serverRepository.GetByServerId(id);

            return await Task.FromResult(server.WelcomeBackground);
        }
    }
}
