using DiscordBot.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBot.DataAccess
{
    public interface IAutoRoleRepository
    {
        Task AddAsync(AutoRole entity);
        Task DeleteAsync(AutoRole entity);
        Task DeleteAutoRole(ulong serverId, ulong roleId);
        Task EditAsync(AutoRole entity);
        Task<List<AutoRole>> GetAutoRoleByServerId(ulong serverId);
    }
}