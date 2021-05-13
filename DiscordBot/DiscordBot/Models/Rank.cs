using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class Rank : DatabaseEntity
    {
        public ulong RoleId { get; set; }
        public ulong ServerId { get; set; }
    }
}
