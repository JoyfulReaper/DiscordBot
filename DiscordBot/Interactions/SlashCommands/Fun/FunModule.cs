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

using Discord.Interactions;
using Discord.WebSocket;
using DiscordBotLibrary.Services.Interfaces;

namespace DiscordBot.Interactions.SlashCommands.Fun;
[Group("fun", "fun commands")]
[RequireContext(ContextType.Guild)]
public class FunModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildService _guildService;
    private readonly IBannerImageService _bannerImageService;

    public FunModule(IGuildService guildService, 
        IBannerImageService bannerImageService)
    {
        _guildService = guildService;
        _bannerImageService = bannerImageService;
    }

    [SlashCommand("banner", "Show the welcome banner for a given user")]
    [RequireContext(ContextType.Guild)]
    public async Task Banner(SocketGuildUser user)
    {
        var background = await _guildService.GetBannerImageAsync(Context.Guild.Id);
        using var memoryStream = await _bannerImageService.CreateImage(user, background);
        memoryStream.Seek(0, SeekOrigin.Begin);
        await RespondWithFileAsync(memoryStream, $"{user.DisplayName}.png");
    }
}
