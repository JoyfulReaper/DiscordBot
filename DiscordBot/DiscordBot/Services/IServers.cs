using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public interface IServers
    {
        Task<string> GetGuildPrefix(ulong id);
        Task ModifyGuildPrefix(ulong id, string prefix);
    }
}