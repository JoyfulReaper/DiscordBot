using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotApi.Dtos
{
    public class GuildCreateDto
    {
        [Required]
        public ulong GuildId { get; set; }

        [Required]
        [MaxLength(100)]
        public string GuildName { get; set; }
    }
}
