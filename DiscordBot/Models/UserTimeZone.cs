using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class UserTimeZone : DatabaseEntity
    {
        public string TimeZone { get; set; }
        public ulong UserId { get; set; }
    }
}
