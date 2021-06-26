using DiscordBotLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLib.Models.DatabaseEntities
{
    public class WarnAction : DatabaseEntity
    {
        public ulong ServerId { get; set; }
        public WarningAction Action { get; set; }
        public int ActionThreshold { get; set; }
    }
}
