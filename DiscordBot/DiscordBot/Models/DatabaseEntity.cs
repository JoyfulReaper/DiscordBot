using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public abstract class DatabaseEntity
    {
        public int Id { get; protected set; }
    }
}
