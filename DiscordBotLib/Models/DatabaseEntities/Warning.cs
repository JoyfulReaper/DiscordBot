using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLib.Models.DatabaseEntities
{
    public class Warning : DatabaseEntity
    {
        public int UserId { get; set; }
        public int ServerId { get; set; }
        public String Text { get; set; }
    }
}
