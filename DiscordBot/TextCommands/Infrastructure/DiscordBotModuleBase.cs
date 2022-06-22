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
using Discord.Commands;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;

namespace DiscordBot.TextCommands.Infrastructure;
public abstract class DiscordBotModuleBase<T> : ModuleBase<T> where T : class, ICommandContext
{
    private readonly IGuildService _guildService;

    public DiscordBotModuleBase(IGuildService guildService)
    {
        _guildService = guildService;
    }

    protected virtual async Task ReplyWithEmbedAsync(string title,
        string? description = null,
        Color? color = null,
        string? thumbImage = null,
        string? imageUrl = null)
    {
        if (color == null)
        {
            color = await _guildService.GetEmbedColorAsync(Context.Guild.Id);
        }
        await ReplyAsync(embed: EmbedHelper.GetEmbed(title, description, color, thumbImage, imageUrl));
    }

    protected virtual async Task SendLogsAsync(string title,
        string description,
        string? thumbnailUrl = null)
    {
        await _guildService.SendLogsAsync(Context.Guild, title, description, thumbnailUrl);
    }
}
