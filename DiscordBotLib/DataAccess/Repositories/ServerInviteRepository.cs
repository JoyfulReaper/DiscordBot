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

using DiscordBotLib.Models.DatabaseEntities;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.Repositories
{
    public class ServerInviteRepository : Repository<ServerInvite>, IServerInviteRepository
    {
        private readonly ISettings _settings;
        private readonly ILogger<ServerInviteRepository> _logger;

        public ServerInviteRepository(ISettings settings, ILogger<ServerInviteRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<List<ServerInvite>> GetServerInvites(ulong server)
        {
            var queryResult = await QueryAsync<ServerInvite>($"SELECT * FROM {TableName} " +
                $"WHERE ServerId = @ServerId", new { ServerId = server });

            return queryResult.ToList();
        }

        public override async Task AddAsync(ServerInvite entity)
        {
            var queryResult = await QuerySingleOrDefaultAsync<ulong>($"INSERT INTO {TableName} " +
                $"(ServerId, Uses, Code) VALUES (@ServerId, @Uses, @Code); " +
                $"select last_insert_rowid();", entity);

            entity.Id = queryResult;
        }

        public override async Task DeleteAsync(ServerInvite entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} " +
                $"WHERE ID = @Id", entity);
        }

        public override async Task EditAsync(ServerInvite entity)
        {
            await ExecuteAsync($"UPDATE {TableName} " +
                $"SET ServerId = @ServerId, Uses = @Uses, Code = @Code " +
                $"WHERE Id = @Id", entity);
        }
    }
}
