using DiscordBotLib.Models.DatabaseEntities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess
{
    public interface IServerInviteRepository
    {
        Task AddAsync(ServerInvite entity);
        Task DeleteAsync(ServerInvite entity);
        Task EditAsync(ServerInvite entity);
        Task<List<ServerInvite>> GetServerInvites(ulong server);
    }
}