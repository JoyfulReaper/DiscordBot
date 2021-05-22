using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class AutoRole : DatabaseEntity
    {
        /// <summary>
        /// Id of the role
        /// </summary>
        public ulong RoleId { get; set; }
        /// <summary>
        /// Id of the server/guild
        /// </summary>
        public ulong ServerId { get; set; }
    }
}
