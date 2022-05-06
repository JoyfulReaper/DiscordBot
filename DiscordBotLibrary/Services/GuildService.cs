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
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Models;
using DiscordBotLibrary.Repositories.Interfaces;
using DiscordBotLibrary.Services.Interfaces;

namespace DiscordBotLibrary.Services;

public class GuildService : IGuildService
{
    private readonly IGuildRepository _guildRepository;

    public GuildService(IGuildRepository guildRepository)
    {
        _guildRepository = guildRepository;
    }

    public Task<Guild> LoadGuild(ulong guildId)
    {
        return _guildRepository.LoadGuildAsync(guildId);
    }
    
    public Task SaveGuild(Guild guild)
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

        var guild = await LoadGuild(guildId.Value);
        if(guild == null || guild.EmbedColor == null || guild.EmbedColor == 0)
        {
            return ColorHelper.RandomColor();
        }

        return new Color(guild.EmbedColor.Value);
    }
}
