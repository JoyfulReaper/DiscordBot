using DiscordBot.DataAccess.SQLite;
using DiscordBot.Models;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.DataAccess.SQLite
{
    public class UserTimeZonesRepository : Repository<UserTimeZone>, IUserTimeZonesRepository
    {
        private readonly ISettings _settings;
        private readonly ILogger<UserTimeZonesRepository> _logger;

        public UserTimeZonesRepository(ISettings settings,
            ILogger<UserTimeZonesRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<UserTimeZone> GetByUserID(ulong userId)
        {
            var queryResult = await QueryFirstOrDefaultAsync<UserTimeZone>($"SELECT * FROM {TableName} " +
                $"WHERE Id = @Id;", new { UserId = userId });

            return queryResult;
        }

        public override async Task AddAsync(UserTimeZone entity)
        {
            await ExecuteAsync($"INSERT INTO {TableName} (UserId, TimeZone) " +
                $"VALUES (@UserId, @TimeZone);", entity);
        }

        public override async Task DeleteAsync(UserTimeZone entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE ID = @Id;", entity);
        }

        public override async Task EditAsync(UserTimeZone entity)
        {
            await ExecuteAsync($"UPDATE {TableName} SET UserId = @UserId, TimeZone = @Timezone " +
                $"WHERE Id = @Id;");
        }
    }
}
