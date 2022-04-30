using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.ConfigSections
{
    public class BotInformation
    {
        public string BotName { get; set; } = "DiscordBot";
        public string BotWebsite { get; set; } = "https://github.com/JoyfulReaper/DiscordBot";
        public string DefaultPrefix { get; set; } = "!";
        public int PrefixMaxLength { get; set; } = 8;
        public string WelcomeMessage { get; set; } = "just spawned in!";
        public string PartMessage { get; set; } = "disappeared forever :(";
        public bool ShowBotJoinMessages { get; set; } = false;
        public int MaxUserNotes { get; set; } = 10;
        public int MaxWelcomeMessages { get; set; } = 10;
        public string InviteLink { get; set; } = "https://discord.com/api/oauth2/authorize?client_id=832404891379957810&permissions=268443670&scope=bot";
    }
}
