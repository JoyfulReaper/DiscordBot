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
    public class UserTimeZoneRepository : Repository<UserTimeZone>, IUserTimeZonesRepository
    {
        private readonly ISettings _settings;
        private readonly ILogger<UserTimeZoneRepository> _logger;

        public UserTimeZoneRepository(ISettings settings,
            ILogger<UserTimeZoneRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<UserTimeZone> GetByUserID(ulong userId)
        {
            var queryResult = await QueryFirstOrDefaultAsync<UserTimeZone>($"SELECT * FROM {TableName} " +
                $"WHERE UserId = @UserId;", new { UserId = userId });

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
            await ExecuteAsync($"UPDATE {TableName} SET TimeZone = @TimeZone " +
                $"WHERE UserId = @UserId;", entity);
        }
    }
}
