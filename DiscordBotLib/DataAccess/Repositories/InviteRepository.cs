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
using System;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.Repositories
{
    public class InviteRepository : Repository<Invite>, IInviteRepository
    {
        private readonly ISettings _settings;
        private readonly ILogger<InviteRepository> _logger;

        public InviteRepository(ISettings settings,
            ILogger<InviteRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<Invite> GetInviteByUser(ulong user, ulong serverId)
        {
            var queryResult = await QuerySingleOrDefaultAsync<Invite>($"SELECT * FROM {TableName} " +
                $"WHERE UserId = @UserId AND ServerId = @ServerId;", new { UserId = user, ServerId = serverId });

            return queryResult;
        }

        public override async Task AddAsync(Invite entity)
        {
            var queryResult = await QuerySingleAsync<ulong>($"INSERT INTO {TableName} " +
                $"(UserId, Count, ServerId) VALUES (@UserId, @Count, @ServerId); select last_insert_rowid();",
                entity);

            entity.Id = queryResult;
        }

        public override async Task DeleteAsync(Invite entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} " +
                $"WHERE Id = @Id;", entity);
        }

        public override async Task EditAsync(Invite entity)
        {
            await ExecuteAsync($"UPDATE {TableName} " +
                $"SET UserId = @UserId, Count = @Count, ServerId = @ServerId " +
                $"WHERE Id = @Id", entity);
        }
    }
}
