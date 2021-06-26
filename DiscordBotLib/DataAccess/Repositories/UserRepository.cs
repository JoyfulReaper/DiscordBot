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
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ISettings _settings;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ISettings settings,
            ILogger<UserRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<User> GetByUserId(ulong userId)
        {
            var queryResult = await QuerySingleOrDefaultAsync<User>($"SELECT * FROM {TableName} " +
                $"WHERE UserId = @UserId;", new { UserId = userId });

            return queryResult;
        }

        public override async Task AddAsync(User entity)
        {
            var queryResult = await QuerySingleOrDefaultAsync<ulong>($"INSERT INTO {TableName} (UserId, UserName) " +
                $"VALUES (@UserId, @UserName); select last_insert_rowid();", entity);

            entity.Id = queryResult;
        }

        public override async Task DeleteAsync(User entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE ID = @Id;", entity);
        }

        public override async Task EditAsync(User entity)
        {
            await ExecuteAsync($"UPDATE {TableName} SET UserName = @UserName " +
                $"WHERE UserId = @UserId", entity);
        }
    }
}
