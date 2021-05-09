using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public Settings(IConfiguration configuration)
        {
            _configuration = configuration;

            Initialize();
        }

        public string OwnerName { get; private set; }
        public string OwnerDiscriminator { get; private set; }
        public string Prefix { get; private set;  }
        public string WelcomeMessage { get; private set; }

        private void Initialize()
        {
            Prefix = _configuration.GetSection("Prefix").Value ?? "!";
            OwnerName = _configuration.GetSection("OwnerName").Value ?? "JoyfulReaper";
            OwnerDiscriminator = _configuration.GetSection("OwnerDiscriminator").Value ?? "7485";
            WelcomeMessage = _configuration.GetSection("WelcomeMessage").Value ?? "just joined!";
        }
    }
}
