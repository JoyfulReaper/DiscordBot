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
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;
using System.Web;

namespace DiscordBot.Interactions.SlashCommands.Web;
[Group("web", "web commands")]
public class WebModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IGuildService _guildService;

    public WebModule(IGuildService guildService)
    {
        _guildService = guildService;
    }

    [SlashCommand("google", "Goolge it")]
    public async Task LetMeGoogleThat(string search)
    {
        await Context.Channel.TriggerTypingAsync();
        var url = "https://www.google.com/search?q=" + HttpUtility.UrlEncode(search);
        await RespondAsync(embed: EmbedHelper.GetEmbed("Google Results", $"I searched google for you:\n{url}",
            await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.SEARCH_IMAGES))));
    }

    [SlashCommand("youtube", "Search youtube")]
    public async Task YouTube(string search)
    {
        await Context.Channel.TriggerTypingAsync();
        var url = "https://www.youtube.com/results?search_query=" + HttpUtility.UrlEncode(search);

        await RespondAsync(embed: EmbedHelper.GetEmbed("YouTube Results", $"I searched youtube for you:\n{url}",
            await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.SEARCH_IMAGES))));
    }

    [SlashCommand("bing", "Search Bing (lol)")]
    public async Task LetMeBingThat(string search)
    {
        await Context.Channel.TriggerTypingAsync();
        
        var url = "https://www.bing.com/search?q=" + HttpUtility.UrlEncode(search);
        await RespondAsync(embed: EmbedHelper.GetEmbed("Google Results", $"I searched Bing for you:\n{url}",
            await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.SEARCH_IMAGES))));
    }
}
