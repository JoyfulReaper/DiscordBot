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
using DiscordBot.DataAccess;
using DiscordBot.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class AutoRoleService
    {
        private readonly IAutoRoleRepository _autoRoleRepository;
        private readonly IServerRepository _serverRepository;
        private readonly Settings _settings;

        public AutoRoleService(IAutoRoleRepository autoRoleRepository,
            IServerRepository serverRepository,
            Settings settings)
        {
            _autoRoleRepository = autoRoleRepository;
            _serverRepository = serverRepository;
            _settings = settings;
        }

        public async Task<List<IRole>> GetAutoRoles(IGuild guild)
        {
            var roles = new List<IRole>();
            var invalidAutoRoles = new List<AutoRole>();

            var autoRoles = await GetAutoRoles(guild.Id);

            foreach (var autoRole in autoRoles)
            {
                var role = guild.Roles.FirstOrDefault(x => x.Id == autoRole.RoleId);

                if (role == null)
                {
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

        public async Task<List<AutoRole>> GetAutoRoles(ulong serverId)
        {
            return await _autoRoleRepository.GetAutoRoleByServerId(serverId);
        }

        public async Task AddAutoRole(ulong serverId, ulong roleId)
        {
            var server = await _serverRepository.GetByServerId(serverId);

            if (server == null)
            {
                await _serverRepository.AddAsync(new Server { GuildId = serverId, Prefix = _settings.DefaultPrefix });
            }

            await _autoRoleRepository.AddAsync(new AutoRole { RoleId = roleId, ServerId = serverId });
        }

        public async Task RemoveAutoRole(ulong serverId, ulong roleId)
        {
            await _autoRoleRepository.DeleteAutoRole(serverId, roleId);
        }

        public async Task ClearAutoRoles(List<AutoRole> autoRoles)
        {
            foreach (AutoRole role in autoRoles)
            {
                await _autoRoleRepository.DeleteAsync(role);
            }
        }
    }
}