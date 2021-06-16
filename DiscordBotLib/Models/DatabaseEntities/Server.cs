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

using Discord;
using DiscordBotLib.Enums;

namespace DiscordBotLib.Models
{
    public class Server : DatabaseEntity
    {
        /// <summary>
        /// Id of the Discord server/Guild
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// Prefix the bot will respond to in the server/guild
        /// </summary>
        public string Prefix { get; set; }

        public bool WelcomeUsers { get; set; } = true;

        /// <summary>
        /// Channel to send welcome messages to
        /// </summary>
        public ulong WelcomeChannel { get; set; }

        /// <summary>
        /// The background image to use for the welcome banner image
        /// </summary>
        public string WelcomeBackground { get; set; }

        /// <summary>
        /// Channel to send logs to
        /// </summary>
        public ulong LoggingChannel { get; set; }

        /// <summary>
        /// Embed color
        /// </summary>
        public Color EmbedColor { get; set; }

        /// <summary>
        /// Allow invites to other discord servers
        /// </summary>
        public bool AllowInvites { get; set; } = true;

        /// <summary>
        /// Determine if profanity should be filtered
        /// </summary>
        public ProfanityFilterMode ProfanityFilterMode { get; set; } = ProfanityFilterMode.FilterOff;
    }
}
