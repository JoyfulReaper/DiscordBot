﻿/*
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
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Models;
using DiscordBotLibrary.Repositories.Interfaces;
using DiscordBotLibrary.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiscordBotLibrary.Services;

public class GuildService : IGuildService
{
    private readonly IGuildRepository _guildRepository;
    private readonly ILogger _logger;

    public GuildService(IGuildRepository guildRepository,
        ILogger logger)
    {
        _guildRepository = guildRepository;
        _logger = logger;
    }

    public Task<Guild> LoadGuildAsync(ulong guildId)
    {
        return _guildRepository.LoadGuildAsync(guildId);
    }
    
    public Task SaveGuildAsync(Guild guild)
    {
        return _guildRepository.SaveGuildAsync(guild);
    }

    public Task<Color> GetEmbedColorAsync(IInteractionContext context)
    {
        var guildId = context.Guild?.Id;
        return GetEmbedColorAsync(guildId);
    }

    public async Task<Color> GetEmbedColorAsync(ulong? guildId)
    {
        if (guildId == null)
        {
            return ColorHelper.RandomColor();
        }

        var guild = await LoadGuildAsync(guildId.Value);
        if(guild == null || guild.EmbedColor == null || guild.EmbedColor == 0)
        {
            return ColorHelper.RandomColor();
        }

        return new Color(guild.EmbedColor.Value);
    }

    public async Task<string?> GetBannerImageAsync(ulong guildId)
    {
        var guild = await _guildRepository.LoadGuildAsync(guildId);
        return guild?.WelcomeBackground;
    }

    public async Task SendLogsAsync(IGuild guild, string title, string description, string? thumbnailUrl = null)
    {
        if (guild == null)
        {
            return;
        }

        var channelId = await GetLoggingChannel(guild.Id);
        if (channelId == null)
        {
            return;
        }

        if (thumbnailUrl == null)
        {
            thumbnailUrl = ImageLookup.GetImageUrl(nameof(ImageLookup.LOGGING_IMAGES));
        }

        var channel = await guild.GetTextChannelAsync(channelId.Value);
        if (channel == null)
        {
            await ClearLoggingChannel(guild.Id);
            return;
        }

        var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(await GetEmbedColorAsync(guild.Id))
                .WithCurrentTimestamp();
        if (thumbnailUrl != null)
        {
            embed.WithThumbnailUrl(thumbnailUrl);
        }

        await channel.SendMessageAsync(embed: embed.Build());
    }

    public async Task ClearLoggingChannel(ulong guildId)
    {
        var guild = await _guildRepository.LoadGuildAsync(guildId);
        if (guild == null)
        {
            _logger.LogDebug("Attempted to clear logging channel for guild {guildId}, but guild was not found", guildId);
            return;
        }

        guild.LoggingChannel = null;
        await _guildRepository.SaveGuildAsync(guild);
    }

    public async Task<ulong?> GetLoggingChannel(ulong guildId)
    {
        var guild = await _guildRepository.LoadGuildAsync(guildId);
        
        if (guild == null)
        {
            await _guildRepository.SaveGuildAsync(new Guild { DiscordGuildId = guildId });
            return null;
        }

        return guild.LoggingChannel;
    }
}
