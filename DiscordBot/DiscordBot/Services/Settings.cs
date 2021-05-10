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

using DiscordBot.DataAccess;
using DiscordBot.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace DiscordBot.Services
{
    public class Settings
    {
        public DatabaseType DatabaseType { get; private set; }
        public string ConnectionString { get; private set; }

        private readonly IConfiguration _configuration;
        private readonly ILogger<Settings> _logger;

        public Settings(IConfiguration configuration,
            ILogger<Settings> logger)
        {
            _configuration = configuration;
            _logger = logger;
            Initialize();
        }

        public string OwnerName { get; private set; }
        public string OwnerDiscriminator { get; private set; }
        public string WelcomeMessage { get; private set; }

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

            OwnerName = _configuration.GetSection("OwnerName").Value ?? "JoyfulReaper";
            OwnerDiscriminator = _configuration.GetSection("OwnerDiscriminator").Value ?? "7485";
            WelcomeMessage = _configuration.GetSection("WelcomeMessage").Value ?? "just joined!";

            SetEmbedColors();            
        }

        private void SetEmbedColors()
        {
            try
            {
                ColorHelper.UseRandomColor = bool.Parse(_configuration.GetSection("RandomEmbedColor").Value);
                _logger.LogDebug("Using random embed colors: {Value}", ColorHelper.UseRandomColor);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception while parsing RandomEmbedColor");
                ColorHelper.UseRandomColor = false;
            }

            if(!ColorHelper.UseRandomColor)
            {
                string color = _configuration.GetSection("EmbedColor").Value;
                var colorValues = color.Split(',', StringSplitOptions.TrimEntries);

                if(colorValues.Length != 3)
                {
                    throw new InvalidOperationException("EmebedColor is not valid");
                }

                int[] iColorValues = new int[3];
                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        iColorValues[i] = int.Parse(colorValues[i]);
                    }
                    ColorHelper.DefaultColor = new Discord.Color(iColorValues[0], iColorValues[1], iColorValues[2]);
                    _logger.LogDebug("Using Color({r},{g},{b})", iColorValues[0], iColorValues[1], iColorValues[2]);
                } 
                catch(Exception ex)
                {
                    _logger.LogWarning(ex, "Exception while parsing EmbedColor. Using 33, 176, 252");
                    ColorHelper.DefaultColor = new Discord.Color(33, 176, 252);
                }
            }
        }
    }
}
