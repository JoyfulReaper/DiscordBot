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

namespace DiscordBot.Services
{
    public interface ISettings
    {
        /// <summary>
        /// Development Guild
        /// </summary>
        ulong DevGuild { get; }

        /// <summary>
        /// Development Channel
        /// Send any global debug messages/logs here
        /// Must be a channel on the DevGuild
        /// </summary>
        ulong DevChannel { get; }

        /// <summary>
        /// Database Connection String
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Database Type
        /// </summary>
        DatabaseType DatabaseType { get; }

        /// <summary>
        /// Default prefix that the bot will respond to
        /// Can be overriden per server
        /// </summary>
        string DefaultPrefix { get; }

        /// <summary>
        /// Bot Owner's Discord Discriminator
        /// </summary>
        string OwnerDiscriminator { get; }

        /// <summary>
        /// Bot Owner's Discord Username
        /// </summary>
        string OwnerName { get; }

        /// <summary>
        /// Welcome Message
        /// </summary>
        string WelcomeMessage { get; }

        /// <summary>
        /// Parting Message
        /// </summary>
        string PartingMessage { get; }

        /// <summary>
        /// Start Lavalink by default or not
        /// </summary>
        bool EnableLavaLink { get; }
    }
}