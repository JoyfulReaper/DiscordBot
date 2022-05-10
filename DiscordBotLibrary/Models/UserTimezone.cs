using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Models;
public class UserTimezone
{
    public long UserTimezoneId{ get; set; }
    public long UserId { get; set; }
    public string TimeZone { get; set; }
    public DateTime DateCreated { get; set; }
}
