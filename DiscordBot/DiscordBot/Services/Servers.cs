using DiscordBot.DataAccess;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class Servers : IServers
    {
        private readonly IServerRepository _serverRepository;
        private readonly IConfiguration _configuration;

        public Servers(IServerRepository serverRepository,
            IConfiguration configuration)
        {
            _serverRepository = serverRepository;
            _configuration = configuration;
        }

        public async Task ModifyGuildPrefix(ulong id, string prefix)
        {
            var server = await _serverRepository.GetByServerId(id);

            if (server == null)
            {
                await _serverRepository.AddAsync(new Models.Server { ServerId = id, Prefix = prefix });
            }
            else
            {
                server.Prefix = prefix;
                await _serverRepository.EditAsync(server);
            }
        }

        public async Task<string> GetGuildPrefix(ulong id)
        {
            string prefix = null;
            var server = await _serverRepository.GetByServerId(id);

            if (server == null)
            {
                prefix = _configuration.GetSection("DefaultPrefix").Value;
            }
            else
            {
                prefix = server.Prefix;
            }

            return prefix;
        }
    }
}
