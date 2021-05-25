using Discord.WebSocket;
using System.Linq;

namespace DiscordBot.Helpers
{
    public static class RoleHelper
    {
        public static bool CheckForRole(SocketGuildUser user, string role)
        {
            if (user.Roles.Any(r => r.Name.ToLowerInvariant() == role.ToLowerInvariant()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}