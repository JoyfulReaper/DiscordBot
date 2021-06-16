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

using DiscordBotLib.Enums;
using DiscordBotLib.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess
{
    public interface IProfanityRepository
    {
        Task AddAsync(Profanity entity);
        Task<Profanity> AddAsync(ulong serverId, string profanity, ProfanityMode mode);
        Task AllowProfanity(ulong serverId, string profanity);
        Task BlockProfanity(ulong serverId, string profanity);
        Task DeleteAsync(Profanity entity);
        Task DeleteAsync(string profanity);
        Task DeleteAsync(ulong serverId, ulong profanity);
        Task EditAsync(Profanity entity);
        Task<List<Profanity>> GetAllowedProfanity(ulong serverId);
        Task<List<Profanity>> GetBlockedProfanity(ulong serverId);
        Task<Profanity> GetProfanity(string word);
    }
}