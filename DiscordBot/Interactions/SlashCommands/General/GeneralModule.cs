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

using Discord.Interactions;
using DiscordBotLibrary.Helpers;
using DiscordBotLibrary.Services.Interfaces;

namespace DiscordBot.Interactions.SlashCommands
{
    [Group("general", "general commands")]
    public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IGuildService _guildService;
        
        public GeneralModule(IGuildService guildService)
        {
            _guildService = guildService;
        }
        
        [SlashCommand("echo", "Echo... Echo... Echo...")]
        public async Task Echo([Summary("Echo", "Echo... Echo... Echo...!")] string input)
        {
            await RespondAsync(null, EmbedHelper.GetEmbedAsArray("Echo...", $"`{input}`"));
        }

        [SlashCommand("ping", "WebSocket server latency")]
        public async Task Ping()
        {
            await RespondAsync(null, EmbedHelper.GetEmbedAsArray("Ping results!", $"Pong! Round-trip latency to Discord WebSocket Server: `{Context.Client.Latency}` ms.",
                await _guildService.GetEmbedColorAsync(Context), ImageLookup.GetImageUrl(nameof(ImageLookup.PING_IMAGES))));
        }
    }
}
