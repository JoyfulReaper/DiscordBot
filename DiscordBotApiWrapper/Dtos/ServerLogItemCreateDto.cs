using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotApiWrapper.Dtos
{
    public class ServerLogItemCreateDto
    {
        public GuildCreateDto Guild { get; set; }
        public ChannelCreateDto Channel { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}
