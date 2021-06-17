using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotApiWrapper.Dtos
{
    public class GuildCreateDto
    {
        public ulong GuildId { get; set; }
        public string GuildName { get; set; }
    }
}
