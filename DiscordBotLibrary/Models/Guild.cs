using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Models
{
    public class Guild
    {
        [Key]
        public long Id { get; set; }
        public UInt64 GuildId { get; set; }
        public UInt64 LoggingChannel { get; set; }
    }
}
