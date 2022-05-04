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
using Discord.WebSocket;

namespace DiscordBotLibrary.Helpers;

public static class ISocketMessageChannelExtensions
{
    public static async Task<IMessage> SendEmbedAsync(this ISocketMessageChannel channel, 
        string title, 
        string description,
        Color? color = null,
        string? thumbImage = null,
        string? imageUrl = null)
    {
        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithCurrentTimestamp();

        if(imageUrl != null)
        {
            embed.WithImageUrl(imageUrl);
        }

        if(color != null)
        {
            embed.WithColor(color.Value);
        }

        if (thumbImage != null)
        {
            embed.WithThumbnailUrl(thumbImage);
        }

        var message = await channel.SendMessageAsync(embed: embed.Build());
        return message;
    }

    //public static async Task<IMessage> SendEmbedAsync(this ISocketMessageChannel channel, 
    //    string title, 
    //    string description, 
    //    Server server,
    //    string thumbImage = null)
    //{
    //    if (server == null)
    //    {
    //        return await SendEmbedAsync(channel, title, description, ColorHelper.RandomColor(), thumbImage);
    //    }

    //    return await SendEmbedAsync(channel, title, description, server.EmbedColor.RawValue == 0 ? ColorHelper.RandomColor() : server.EmbedColor, thumbImage);
    //}
}