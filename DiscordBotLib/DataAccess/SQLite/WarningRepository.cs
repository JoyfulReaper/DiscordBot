using DiscordBotLib.Enums;
using DiscordBotLib.Models;
using DiscordBotLib.Models.DatabaseEntities;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.SQLite
{
    public class WarningRepository : Repository<Warning>
    {
        private readonly ISettings _settings;
        private readonly ILogger<WarningRepository> _logger;

        public WarningRepository(ISettings settings,
            ILogger<WarningRepository> logger) : base (settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task SetWarnAction(WarningAction action, Server server)
        {
            int count = await QueryFirstOrDefaultAsync<int>($"SELECT count(Id) " +
                $"FROM WarningAction WHERE ServerId = @ServerId;",
                new { ServerId = server.Id });

            if (count != 0)
            {
                await ExecuteAsync($"UPDATE WarningAction " +
                     $"SET ServerId = @ServerId, Action = @Action " +
                     $"WHERE ServerId = @ServerId",
                     new { ServerId = server.Id, Action = (int)action });
            }
            else
            {
                await ExecuteAsync($"INSERT INTO WarningAction " +
                    $"(ServerId, Action) " +
                    $"VALUES (@ServerId, @Action);",
                    new { ServerId = server.Id, Action = (int)action });
            }
        }

        public async Task<WarningAction> GetWarningAction(Server server)
        {
            var action = await QueryFirstOrDefaultAsync<WarningAction>("SELECT Action FROM " +
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
