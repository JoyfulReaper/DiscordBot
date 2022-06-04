using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Models
{
    public class BotSetting
    {
        [Key]
        public long BotSettingId { get; set; }

        public string Token { get; set; } = string.Empty;

        public string? Game { get; set; }
    }
}
