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
    /// Auto Roles are roles that are assigned to users automaticly upon
    /// joining the server.
    /// </summary>
    public class AutoRoleService : IAutoRoleService
    {
        private readonly IAutoRoleRepository _autoRoleRepository;
        private readonly IServerRepository _serverRepository;
        private readonly ISettings _settings;

        public AutoRoleService(IAutoRoleRepository autoRoleRepository,
            IServerRepository serverRepository,
            ISettings settings)
        {
            _autoRoleRepository = autoRoleRepository;
            _serverRepository = serverRepository;
            _settings = settings;
        }

        /// <summary>
        /// Get a list of auto roles for this server
        /// </summary>
        /// <param name="guild">The server for which to retereive the auto roles</param>
        /// <returns>A list of auto roles for the given server</returns>
        public async Task<IEnumerable<IRole>> GetAutoRoles(IGuild guild)
        {
            var roles = new List<IRole>();
            var invalidAutoRoles = new List<AutoRole>();

            var autoRoles = await GetAutoRoles(guild.Id);
            if(autoRoles == null)
            {
                return null;
            }

            foreach (var autoRole in autoRoles)
            {
                var role = guild.Roles.FirstOrDefault(x => x.Id == autoRole.RoleId);

                if (role == null)
                {
                    // If the role doesn't exist any more
                    // Or isn't valid for some other reason
                    // add it to a list to be removed.
                    invalidAutoRoles.Add(autoRole);
                }
                else
                {
                    var currentUser = await guild.GetCurrentUserAsync();
                    var hierachy = (currentUser as SocketGuildUser).Hierarchy;

                    if (role.Position > hierachy)
                    {
                        invalidAutoRoles.Add(autoRole);
                    }
                    else
                    {
                        roles.Add(role);
                    }
                }
            }

            if (invalidAutoRoles.Count > 0)
            {
                await ClearAutoRoles(invalidAutoRoles);
            }

            return roles;
        }

        /// <summary>
        /// Get a list of auto roles for this server
        /// </summary>
        /// <param name="serverId">The id of the server for which to retereive the auto roles</param>
        /// <returns>A list of auto roles for the given server</returns>
        public async Task<IEnumerable<AutoRole>> GetAutoRoles(ulong serverId)
        {
            return await _autoRoleRepository.GetAutoRoleByServerId(serverId);
        }

        /// <summary>
        /// Add an auto role for the given server
        /// </summary>
        /// <param name="serverId">The id of the sercer</param>
        /// <param name="roleId">The id of the role to add</param>
        /// <returns></returns>
        public async Task AddAutoRole(ulong serverId, ulong roleId)
        {
            var server = await _serverRepository.GetByServerId(serverId);

            if (server == null)
            {
                await _serverRepository.AddAsync(new Server { GuildId = serverId, Prefix = _settings.DefaultPrefix });
            }

            await _autoRoleRepository.AddAsync(new AutoRole { RoleId = roleId, ServerId = server.Id });
        }

        /// <summary>
        /// Remove an auto role for the given server
        /// </summary>
        /// <param name="serverId">Id of the server</param>
        /// <param name="roleId">Id of the role to remove</param>
        /// <returns></returns>
        public async Task RemoveAutoRole(ulong serverId, ulong roleId)
        {
            var server = await _serverRepository.GetByServerId(serverId);

            if (server == null)
            {
                await _serverRepository.AddAsync(new Server { GuildId = serverId, Prefix = _settings.DefaultPrefix });
            }

            await _autoRoleRepository.DeleteAutoRole(server.Id, roleId);
        }

        /// <summary>
        /// Delete a list of auto roles from the DB
        /// </summary>
        /// <param name="autoRoles">The list of auto roles to delete</param>
        /// <returns></returns>
        public async Task ClearAutoRoles(List<AutoRole> autoRoles)
        {
            foreach (AutoRole role in autoRoles)
            {
                await _autoRoleRepository.DeleteAsync(role);
            }
        }
    }
}