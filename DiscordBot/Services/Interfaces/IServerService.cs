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
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public interface IServerService
    {
        Task ClearBackground(ulong id);
        Task ClearWelcomeChannel(ulong id);
        Task<string> GetBackground(ulong id);
        Task<string> GetGuildPrefix(ulong id);
        Task<ulong> GetWelcomeChannel(ulong id);
        Task ModifyGuildPrefix(ulong id, string prefix);
        Task ModifyWelcomeBackground(ulong id, string url);
        Task ModifyWelcomeChannel(ulong id, ulong channelId);
        Task ModifyLoggingChannel(ulong id, ulong channelId);
        Task ClearLoggingChannel(ulong id);
        Task<ulong> GetLoggingChannel(ulong id);
        Task SendLogsAsync(IGuild guild, string title, string description, string loggingThumbnail = null);
        Task ModifyEmbedColor(ulong id, string color);
        Task<Color> GetEmbedColor(ulong id);

    }
}