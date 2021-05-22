using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
    public class Mute
    {
        public SocketGuild Guild { get; set; }
        public SocketGuildUser User { get; set; }
        public IRole Role { get; set; }
        public DateTime End { get; set; }
    }
}
