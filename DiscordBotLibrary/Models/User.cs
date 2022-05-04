using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Models
{
    public class User
    {
        [Key]
        public long Id { get; set; }

        public UInt64 UserId { get; set; }

        public string UserName { get; set; } = string.Empty;
    }
}
