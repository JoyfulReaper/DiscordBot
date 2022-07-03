using DiscordBotLibrary.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Models
{
    public class WarningAction
    {
        public long WarningActionId { get; set; }
        public long GuildId { get; set; }
        public WarnAction Action { get; set; }
        public int ActionThreshold { get; set; }
    }
}
