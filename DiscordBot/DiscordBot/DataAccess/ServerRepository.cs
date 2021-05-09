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

using DiscordBot.Models;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DiscordBot.DataAccess
{
    public class ServerRepository : Repository<Server>, IServerRepository
    {
        private readonly Settings _settings;
        private readonly ILogger<ServerRepository> _logger;

        public ServerRepository(Settings settings,
            ILogger<ServerRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<Server> GetByServerId(ulong serverId)
        {
            return await QuerySingleOrDefaultAsync<Server>($"SELECT * FROM {TableName} WHERE ServerId = @ServerId", new { ServerId = serverId });
        }

        public async override Task AddAsync(Server entity)
        {
            await ExecuteAsync($"INSERT INTO ${TableName} (ServerId, Prefix)" +
                "VALUES (@ServerId, @Prefix);", new { entity.ServerId, entity.Prefix });
        }

        public async override Task DeleteAsync(Server entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE Id=@Id;", new { Id = entity.Id });
        }

        public async override Task EditAsync(Server entity)
        {
            await ExecuteAsync($"UPDATE {TableName} SET Prefix = @Prefix, ServerId = @ServerId" +
                $"WHERE Id = @Id;", entity);
        }
    }
}
