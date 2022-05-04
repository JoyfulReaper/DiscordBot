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
        public decimal GuildId { get; set; }
        public decimal LoggingChannel { get; set; }
    }
}
