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
using DiscordBotLib.Models;
using System.Threading.Tasks;

namespace DiscordBotLib.Services
{
    public interface IServerService
    {
        /// <summary>
        /// Get the Server model
        /// </summary>
        /// <param name="guild">Guild to retreive the model for</param>
        /// <returns>Server model for guild</returns>
        Task<Server> GetServer(IGuild guild);

        /// <summary>
        /// Update server model
        /// </summary>
        /// <param name="server">Server model to save to the database</param>
        /// <returns></returns>
        Task UpdateServer(Server server);

        /// <summary>
        /// Clear Banner image background
        /// </summary>
        /// <param name="id">Guild Id</param>
        /// <returns></returns>
        Task ClearBackground(ulong id);

        /// <summary>
        /// Clear Welcome Channel
        /// </summary>
        /// <param name="id">Guild Id</param>
        /// <returns></returns>
        Task ClearWelcomeChannel(ulong id);

        /// <summary>
        /// Get Banner image background
        /// </summary>
        /// <param name="id">Guild Id</param>
        /// <returns>Background image url as string</returns>
        Task<string> GetBackground(ulong id);

        /// <summary>
        /// Get the bot prefix
        /// </summary>
        /// <param name="id">Guild Id</param>
        /// <returns>Bot prefix for guild</returns>
        Task<string> GetGuildPrefix(ulong id);

        /// <summary>
        /// Get the welcome channel
        /// </summary>
        /// <param name="id">Guild Id</param>
        /// <returns>Welcome channel id</returns>
        Task<ulong> GetWelcomeChannel(ulong id);

        /// <summary>
        /// Update the bot prefix
        /// </summary>
        /// <param name="id">Guild Id</param>
        /// <param name="prefix">New prefix</param>
        /// <returns></returns>
        Task ModifyGuildPrefix(ulong id, string prefix);

        /// <summary>
        /// Update welcome background image
        /// </summary>
        /// <param name="id">Guild id</param>
        /// <param name="url">URL of background image</param>
        /// <returns></returns>
        Task ModifyWelcomeBackground(ulong id, string url);

        /// <summary>
        /// Update the welcome channel
        /// </summary>
        /// <param name="id">Guild Id</param>
        /// <param name="channelId">Channel id of welcome channel</param>
        /// <returns></returns>
        Task ModifyWelcomeChannel(ulong id, ulong channelId);

        /// <summary>
        /// Update channel to send server level logs to
        /// </summary>
        /// <param name="id">Guild id</param>
        /// <param name="channelId">log channel id</param>
        /// <returns></returns>
        Task ModifyLoggingChannel(ulong id, ulong channelId);

        /// <summary>
        /// Clear the logging channel
        /// </summary>
        /// <param name="id">Guild Id</param>
        /// <returns></returns>
        Task ClearLoggingChannel(ulong id);

        /// <summary>
        /// Get the logging channel
        /// </summary>
        /// <param name="id">guild Id</param>
        /// <returns>Logging channel for guild</returns>
        Task<ulong> GetLoggingChannel(ulong id);

        /// <summary>
        /// Send logs to logging channel
        /// </summary>
        /// <param name="guild">Guild</param>
        /// <param name="title">Embed title</param>
        /// <param name="description">Embed description</param>
        /// <param name="loggingThumbnail">Thumbnail to show with the embed</param>
        /// <returns></returns>
        Task SendLogsAsync(IGuild guild, string title, string description, string loggingThumbnail = null);

        /// <summary>
        /// Change the Embed color
        /// </summary>
        /// <param name="id">Guild Id</param>
        /// <param name="color">Color as rbg comma seperated or 0 for random</param>
        /// <returns></returns>
        Task ModifyEmbedColor(ulong id, string color);

        /// <summary>
        /// Get the embed color
        /// </summary>
        /// <param name="id">Server Id</param>
        /// <returns>Discord Color for the embeds</returns>
        Task<Color> GetEmbedColor(ulong id);

        /// <summary>
        /// True if using random embed colors, false otherwise
        /// </summary>
        /// <param name="serverId">Guild id</param>
        /// <returns>True if using random embed colors, false otherwise</returns>
        Task<bool> UsingRandomEmbedColor(ulong serverId);
    }
}