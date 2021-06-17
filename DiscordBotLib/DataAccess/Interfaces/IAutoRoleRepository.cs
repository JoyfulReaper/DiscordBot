using DiscordBotLib.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess
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