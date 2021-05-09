using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class Server : DatabaseEntity
    {
        public ulong ServerId { get; set; }
        public string Prefix { get; set; }
    }
}
