using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public interface IServerService
    {
        Task<string> GetGuildPrefix(ulong id);
        Task ModifyGuildPrefix(ulong id, string prefix);
    }
}