/*
MIT License

Copyright(c) 2022 Kyle Givler
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
using DiscordBotLibrary.Models;

namespace DiscordBotLibrary.Services.Interfaces;

public interface IGuildService
{
    /// <summary>
    /// Load a guild from the database
    /// If the guild cannot be found a new record will be inserted
    /// </summary>
    /// <param name="guildId">The snowflake id of the guild</param>
    /// <returns>The guild record from the database, or the newly created record if one did not exist</returns>
    Task<Guild> LoadGuildAsync(ulong GuildId);

    /// <summary>
    /// Persist changes to a guild
    /// </summary>
    /// <param name="guild"></param>
    /// <returns></returns>
    Task SaveGuildAsync(Guild guild);


    
    /// <summary>
    /// Get the color to use for embeds for the given Guild
    /// </summary>
    /// <param name="GuildId">The snowflake id of the guild</param>
    /// <returns>The embed color for the given guild, or a random color if not specified</returns>
    Task<Color> GetEmbedColorAsync(ulong? GuildId);

    /// <summary>
    /// Get the color to use for embeds for the given Context
    /// </summary>
    /// <param name="context">The IInteractionContext to determine the embed color for</param>
    /// <returns>The embed color for the given IInteractionContext, or a random color if not specified</returns>
    Task<Color> GetEmbedColorAsync(IInteractionContext context);

    
    Task<string?> GetBannerImageAsync(ulong guildId);

    /// <summary>
    /// Send a logging message to the logging channel
    /// </summary>
    /// <param name="guild">Guild to log for</param>
    /// <param name="title">Title of the log message</param>
    /// <param name="description">Content of the log message</param>
    /// <param name="thumbnailUrl">Image to show in the logging embed</param>
    /// <returns></returns>
    Task SendLogsAsync(IGuild guild, string title, string description, string? thumbnailUrl = null);

    /// <summary>
    /// Clears out the logging channel
    /// </summary>
    /// <param name="guildId">Snowflake Id of the Guild</param>
    /// <returns></returns>
    Task ClearLoggingChannelAsync(ulong guildId);

    /// <summary>
    /// Get the snowflake Id of the logging channel
    /// </summary>
    /// <param name="guildId">Snowflake id of the guild</param>
    /// <returns>Logging channel snowflake id</returns>
    Task<ulong?> GetLoggingChannelAsync(ulong guildId);

    /// <summary>
    /// Set the logging channel for the given guild
    /// </summary>
    /// <param name="guildId">Snowflake id of the server</param>
    /// <param name="channelId">Snowflake id of the logging channel</param>
    /// <returns></returns>
    Task SetLoggingChannelAsync(ulong guildId, ulong channelId);



    /// <summary>
    /// Get the prefix for the given guild
    /// </summary>
    /// <param name="guildId">Snowflake Id of the guild</param>
    /// <returns>The guilds prefix</returns>
    Task<string> GetGuildPrefixAsync(ulong guildId);

    /// <summary>
    /// Set the prefic for the given guild
    /// </summary>
    /// <param name="guildId">Snowflake Id of the guild</param>
    /// <param name="prefix">The new prefix</param>
    /// <returns></returns>
    Task SetGuildPrefixAsync(ulong guildId, string prefix);
}