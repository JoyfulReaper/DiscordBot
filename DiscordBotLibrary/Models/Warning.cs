using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Models
{
    public class Warning
    {
        public long WarningId { get; set; }
        public long UserId { get; set; }
        public long GuildId { get; set; }
        public string? Text { get; set; }
        public DateTime? DateCleared { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
