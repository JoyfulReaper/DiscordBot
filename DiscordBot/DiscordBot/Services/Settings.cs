using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    //TODO read settings from appsettings.json
    public class Settings
    {
        public string OwnerName { get; } = "JoyfulReaper";
        public string OwnerDiscriminator { get; }  = "7485";
    }
}
