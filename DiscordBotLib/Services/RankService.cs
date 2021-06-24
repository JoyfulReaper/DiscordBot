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
using Discord.WebSocket;
using DiscordBotLib.DataAccess;
using DiscordBotLib.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotLib.Services
{
    /// <summary>
    /// Ranks are roles the user can choose to assign to themselves
    /// </summary>
    public class RankService : IRankService
    {
        private readonly IRankRepository _rankRepository;
        private readonly IServerRepository _serverRepository;
        private readonly ISettings _settings;

        public RankService(IRankRepository rankRepository,
            IServerRepository serverRepository,
            ISettings settings)
        {
            _rankRepository = rankRepository;
            _serverRepository = serverRepository;
            _settings = settings;
        }

        /// <summary>
        /// Get a list of ranks for this server
        /// </summary>
        /// <param name="guild">The server for which to retereive the ranks</param>
        /// <returns>A list of ranks for the given server</returns>
        public async Task<List<IRole>> GetRanks(IGuild guild)
        {
            var roles = new List<IRole>();
            var invalidRanks = new List<Rank>();

            var ranks = await GetRanks(guild.Id);

            foreach (var rank in ranks)
            {
                var role = guild.Roles.FirstOrDefault(x => x.Id == rank.RoleId);

                if (role == null)
                {
                    invalidRanks.Add(rank);
                }
                else
                {
                    var currentUser = await guild.GetCurrentUserAsync();
                    var hierachy = (currentUser as SocketGuildUser).Hierarchy;

                    if (role.Position > hierachy)
                    {
                        invalidRanks.Add(rank);
                    }
                    else
                    {
                        roles.Add(role);
                    }
                }
            }

            if (invalidRanks.Count > 0)
            {
                await ClearRanks(invalidRanks);
            }

            return roles;
        }

        /// <summary>
        /// Get a list of ranks for this server
        /// </summary>
        /// <param name="serverId">The server for which to retereive the ranks</param>
        /// <returns>A list of ranks for the given server</returns>
        public async Task<List<Rank>> GetRanks(ulong serverId)
        {
            return await _rankRepository.GetRanksByServerId(serverId);
        }

        /// <summary>
        /// Add a rank for the given server
        /// </summary>
        /// <param name="serverId">The id of the sercer</param>
        /// <param name="roleId">The id of the rank to add</param>
        /// <returns></returns>
        public async Task AddRank(ulong serverId, ulong roleId)
        {
            var server = await _serverRepository.GetByServerId(serverId);

            if (server == null)
            {
                await _serverRepository.AddAsync(new Server { GuildId = serverId, Prefix = _settings.DefaultPrefix });
            }

            await _rankRepository.AddAsync(new Rank { RoleId = roleId, ServerId = server.Id });
        }

        /// <summary>
        /// Remove a rank for the given server
        /// </summary>
        /// <param name="serverId">Id of the server</param>
        /// <param name="roleId">Id of the rank to remove</param>
        /// <returns></returns>
        public async Task RemoveRank(ulong serverId, ulong roleId)
        {
            await _rankRepository.DeleteRank(serverId, roleId);
        }


        /// <summary>
        /// Delete a list of ranks from the DB
        /// </summary>
        /// <param name="autoRoles">The list of ranks to delete</param>
        /// <returns></returns>
        public async Task ClearRanks(List<Rank> ranks)
        {
            foreach (Rank rank in ranks)
            {
                await _rankRepository.DeleteAsync(rank);
            }
        }
    }
}