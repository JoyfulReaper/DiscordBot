using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLib.Models.DatabaseEntities
{
    public class ServerInvite : DatabaseEntity
    {
        public int Uses { get; set; }
        public ulong ServerId { get; set; }
        public string Code { get; set; }
    }
}
