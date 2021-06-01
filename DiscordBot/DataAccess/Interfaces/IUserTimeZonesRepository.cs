using DiscordBot.Models;
using System.Threading.Tasks;

namespace DiscordBot.DataAccess
{
    public interface IUserTimeZonesRepository
    {
        Task AddAsync(UserTimeZone entity);
        Task DeleteAsync(UserTimeZone entity);
        Task EditAsync(UserTimeZone entity);
        Task<UserTimeZone> GetByUserID(ulong userId);
    }
}