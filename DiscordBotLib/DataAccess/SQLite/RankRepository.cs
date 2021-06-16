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

using DiscordBotLib.Models;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.SQLite
{
    public class RankRepository : Repository<Rank>, IRankRepository
    {
        private readonly ISettings _settings;
        private readonly ILogger<RankRepository> _logger;

        public RankRepository(ISettings settings,
        ILogger<RankRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<List<Rank>> GetRanksByServerId(ulong serverId)
        {
            var queryResult = await QueryAsync<Rank>($"SELECT * FROM {TableName} WHERE ServerId = @ServerId;", new { ServerId = serverId });
            return queryResult.ToList();
        }

        public async override Task AddAsync(Rank entity)
        {
            await ExecuteAsync($"INSERT INTO {TableName} (ServerId, RoleId) " +
                $"VALUES (@ServerId, @RoleId);", new { ServerId = entity.ServerId, RoleId = entity.RoleId });
        }

        public async override Task DeleteAsync(Rank entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE ID = @Id;", new { Id = entity.Id });
        }

        public async Task DeleteRank(ulong serverId, ulong roleId)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE ServerId = @ServerId AND roleId = @RoleId;",
                new { ServerId = serverId, RoleId = roleId });
        }

        public async override Task EditAsync(Rank entity)
        {
            await ExecuteAsync($"UPDATE {TableName} SET ServerId = @ServerId, RoleId=@RoleId " +
                $"WHERE Id = @Id;", entity);
        }
    }
}
