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
using DiscordBotLib.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBotLib.Services
{
    public interface IRankService
    {
        /// <summary>
        /// Add a user assignable rank
        /// </summary>
        /// <param name="serverId">Guild Id</param>
        /// <param name="roleId">Discord role of the rank</param>
        /// <returns></returns>
        Task AddRank(ulong serverId, ulong roleId);

        /// <summary>
        /// Clear given ranks
        /// </summary>
        /// <param name="ranks">The ranks to clear</param>
        /// <returns></returns>
        Task ClearRanks(List<Rank> ranks);

        /// <summary>
        /// Get avaiable ranks
        /// </summary>
        /// <param name="guild">Guild</param>
        /// <returns>Roles avaiable to use as ranks in the guild</returns>
        Task<List<IRole>> GetRanks(IGuild guild);

        /// <summary>
        /// Get avaiable ranks
        /// </summary>
        /// <param name="serverId">server id</param>
        /// <returns>Ranks avaiable for the given server id</returns>
        Task<List<Rank>> GetRanks(ulong serverId);

        /// <summary>
        /// Remove a rank
        /// </summary>
        /// <param name="serverId">Server id</param>
        /// <param name="roleId">id of the role for the rank to remove</param>
        /// <returns></returns>
        Task RemoveRank(ulong serverId, ulong roleId);
    }
}