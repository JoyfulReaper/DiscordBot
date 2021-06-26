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
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.Repositories
{
    public class WarningRepository : Repository<Warning>, IWarningRepository
    {
        private readonly ISettings _settings;
        private readonly ILogger<WarningRepository> _logger;

        public WarningRepository(ISettings settings,
            ILogger<WarningRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<IEnumerable<Warning>> GetUsersWarnings(Server server, User user)
        {
            var queryResult = await QueryAsync<Warning>("SELECT * FROM Warning " +
                "WHERE UserId = @UserId AND ServerId = @ServerId", new { ServerId = server.Id, UserId = user.Id});

            return queryResult;
        }

        public async Task SetWarnAction(WarnAction action)
        {
            int count = await QueryFirstOrDefaultAsync<int>($"SELECT count(Id) " +
                $"FROM WarningAction WHERE ServerId = @ServerId;",
                new { ServerId = action.ServerId });

            if (count != 0)
            {
                await ExecuteAsync($"UPDATE WarningAction " +
                     $"SET ServerId = @ServerId, Action = @Action, ActionThreshold = @ActionThreshold " +
                     $"WHERE ServerId = @ServerId;",
                     new { ServerId = action.ServerId, Action = (int)action.Action, ActionThreshold = action.ActionThreshold });
            }
            else
            {
                await ExecuteAsync($"INSERT INTO WarningAction " +
                    $"(ServerId, Action, ActionThreshold) " +
                    $"VALUES (@ServerId, @Action, @ActionThreshold);",
                    new { ServerId = action.ServerId, Action = (int)action.Action, ActionThreshold = action.ActionThreshold });
            }
        }

        public async Task<WarnAction> GetWarningAction(Server server)
        {
            var action = await QueryFirstOrDefaultAsync<WarnAction>("SELECT * FROM " +
                "WarningAction WHERE @ServerId = ServerId", new { ServerId = server.Id });

            return action;
        }

        public async override Task AddAsync(Warning entity)
        {
            var queryResult = await QuerySingleAsync<ulong>($"INSERT INTO {TableName} (UserId, ServerId, Text) " +
                $"VALUES (@UserId, @ServerId, @Text); select last_insert_rowid();",
                entity);

            entity.Id = queryResult;
        }

        public async override Task DeleteAsync(Warning entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE Id = @id;",
                entity);
        }

        public async override Task EditAsync(Warning entity)
        {
            await ExecuteAsync($"UPDATE {TableName} SET ServerId = @ServerId, UserId = @UserId, Text = @Text " +
                $"WHERE Id = @Id;", entity);
        }
    }
}
