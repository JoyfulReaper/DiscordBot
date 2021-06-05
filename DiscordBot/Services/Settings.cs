/*
MIT License

Copyright(c) 2021 Kyle Givler
https://github.com/JoyfulReaper

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using DiscordBot.DataAccess.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace DiscordBot.Services
{
    public class Settings : ISettings
    {
        public ulong DevGuild { get; set; }
        public ulong DevChannel { get; set; }
        public DatabaseType DatabaseType { get; private set; }
        public string ConnectionString { get; private set; }
        public string OwnerName { get; private set; }
        public string OwnerDiscriminator { get; private set; }
        public string WelcomeMessage { get; private set; }
        public string DefaultPrefix { get; private set; }
        public bool EnableLavaLink
        {
            get => enableLavaLink;
            set { enableLavaLink = value; }
        }

        private bool enableLavaLink;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Settings> _logger;

        public Settings(IConfiguration configuration,
            ILogger<Settings> logger)
        {
            _configuration = configuration;
            _logger = logger;

            Initialize();
        }

        private void Initialize()
        {
            var databaseType = _configuration.GetSection("DatabaseType").Value;

            if (databaseType == "SQLite")
            {
                DatabaseType = DatabaseType.SQLite;
                ConnectionString = _configuration.GetConnectionString("SQLite");
            }
            else
            {
                throw new InvalidOperationException("DatabaseType is not valid.");
            }

            if (!bool.TryParse(_configuration.GetSection("StartLavaLink").Value, out enableLavaLink))
            {
                _logger.LogWarning("Unable to parse StartLavaLink, using {value}", EnableLavaLink);
            }

            OwnerName = _configuration.GetSection("OwnerName").Value ?? "JoyfulReaper";
            OwnerDiscriminator = _configuration.GetSection("OwnerDiscriminator").Value ?? "7485";
            WelcomeMessage = _configuration.GetSection("WelcomeMessage").Value ?? "just joined!";
            DefaultPrefix = _configuration.GetSection("DefaultPrefix").Value ?? "!";

            try
            {
                DevGuild = UInt64.Parse(_configuration.GetSection("DevGuild").Value);
                DevChannel = UInt64.Parse(_configuration.GetSection("DevChannel").Value);
            }
            catch
            {
                DevGuild = 0;
                DevChannel = 0;
                _logger.LogWarning("Unable to parse DevGuild or DevChannel, using '0'");
            }
        }
    }
}
