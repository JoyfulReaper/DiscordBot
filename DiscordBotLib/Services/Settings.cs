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

using DiscordBotLib.DataAccess.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SQLite;

namespace DiscordBotLib.Services
{
    public class Settings : ISettings
    {
        public string BotName { get; set; }

        public string BotWebsite { get; set; }

        public string InviteLink { get; private set; }

        public Type DbConnectionType { get; private set; }

        /// <summary>
        /// Guild to send testing/debug messages to
        /// </summary>
        public ulong DevGuild { get; set; }

        /// <summary>
        /// Channel in gev guild to send testing/debug messages to
        /// </summary>
        public ulong DevChannel { get; set; }

        /// <summary>
        /// The datebase to use
        /// </summary>
        public DatabaseType DatabaseType { get; private set; }

        /// <summary>
        /// The database connection string
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// The username of the bot owner
        /// </summary>
        public string OwnerName { get; private set; }

        /// <summary>
        /// The discriminator of the bot owner
        /// </summary>
        public string OwnerDiscriminator { get; private set; }

        /// <summary>
        /// The UserId of the bot owner
        /// </summary>
        public ulong OwnerUserId { get; private set; }

        /// <summary>
        /// Message to send when a user joins. TODO make this per server
        /// </summary>
        public string WelcomeMessage { get; private set; }

        /// <summary>
        /// Message to show when a user leaves. TODO make this per server
        /// </summary>
        public string PartingMessage { get; private set; }

        /// <summary>
        /// Default bot prefix
        /// </summary>
        public string DefaultPrefix { get; private set; }

        /// <summary>
        /// Base url of the DiscordBotApi
        /// </summary>
        public string ApiBaseAddress { get; set; }

        public string ApiUserName { get; set; }

        public string ApiPassword { get; set; }

        public int MaxUserNotes { get; set; }

        public int MaxWelcomeMessages { get; set; }

        /// <summary>
        /// True to intergrate with DiscordBotApi, fale to not intergrate
        /// </summary>
        public bool UseDiscordBotApi
        {
            get => _useDiscordBotApi;
            set { _useDiscordBotApi = value; }
        }

        /// <summary>
        /// True to start Lavalink jar when the bot starts
        /// </summary>
        public bool EnableLavaLink
        {
            get => _enableLavaLink;
            set { _enableLavaLink = value; }
        }

        private bool _enableLavaLink;
        private bool _useDiscordBotApi;
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
                DbConnectionType = typeof(SQLiteConnection);
            }
            else
            {
                throw new InvalidOperationException("DatabaseType is not valid.");
            }

            if (!bool.TryParse(_configuration.GetSection("StartLavaLink").Value, out _enableLavaLink))
            {
                _logger.LogWarning("Unable to parse StartLavaLink, using {value}", EnableLavaLink);
            }

            if (!bool.TryParse(_configuration.GetSection("UseDiscordBotApi").Value, out _useDiscordBotApi))
            {
                _logger.LogWarning("Unable to parse UseDiscordBotApi, using {value}", UseDiscordBotApi);
            }

            OwnerName = _configuration.GetSection("OwnerName").Value ?? "JoyfulReaper";
            OwnerDiscriminator = _configuration.GetSection("OwnerDiscriminator").Value ?? "7485";
            if (!ulong.TryParse(_configuration.GetSection("OwnerUserId").Value, out ulong ownerId))
            {
                OwnerUserId = 397107333341118468;
                _logger.LogWarning("Unable to parse OwnerUserId, using {value}", OwnerUserId);
            }
            else
            {
                OwnerUserId = ownerId;
            }

            ApiBaseAddress = _configuration.GetSection("ApiBaseAddress").Value ?? "https://localhost:5001";
            ApiUserName = _configuration.GetSection("ApiUserName").Value ?? String.Empty;
            ApiPassword = _configuration.GetSection("ApiPassword").Value ?? String.Empty;

            BotName = _configuration.GetSection("BotName").Value ?? "DiscordBot";
            BotWebsite = _configuration.GetSection("BotWebsite").Value ?? "https://github.com/JoyfulReaper/DiscordBot";
            WelcomeMessage = _configuration.GetSection("WelcomeMessage").Value ?? "just joined!";
            PartingMessage = _configuration.GetSection("PartingMessage").Value ?? "just bailed!";
            DefaultPrefix = _configuration.GetSection("DefaultPrefix").Value ?? "!";
            InviteLink = _configuration.GetSection("InviteLink").Value ?? "https://discord.com/api/oauth2/authorize?client_id=832404891379957810&permissions=268443670&scope=bot";

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

            try
            {
                MaxUserNotes = int.Parse(_configuration.GetSection("MaxUserNotes").Value);
            }
            catch
            {
                MaxUserNotes = 1;
                _logger.LogWarning("Unable to parse MaxUserNotes, using: {MaxUserNotes}", MaxUserNotes);
            }

            if (!int.TryParse(_configuration.GetSection("MaxWelcomeMessages").Value, out int max))
            {
                MaxWelcomeMessages = 10;
                _logger.LogWarning("Unable to parse MaxWelcomeMessages, using {value}", MaxWelcomeMessages);
            }
            else
            {
                MaxWelcomeMessages = max;
            }
        }
    }
}
