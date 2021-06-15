using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotApi.Dtos
{
    public class ChannelCreateDto
    {
        [Required]
        public ulong ChannelId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ChannelName { get; set; }
    }
}
