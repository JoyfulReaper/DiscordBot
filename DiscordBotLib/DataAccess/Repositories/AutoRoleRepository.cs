using DiscordBotLib.Models;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.Repositories
{
    public class AutoRoleRepository : Repository<AutoRole>, IAutoRoleRepository
    {
        private readonly ISettings _settings;
        private readonly ILogger<AutoRoleRepository> _logger;

        public AutoRoleRepository(ISettings settings,
            ILogger<AutoRoleRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<IEnumerable<AutoRole>> GetAutoRoleByServerId(ulong serverId)
        {
            var queryResult = await QueryAsync<AutoRole>($"SELECT a.RoleId, a.Id, a.ServerId " +
                $"FROM {TableName} a " +
                $"INNER JOIN Server s ON a.ServerId = s.Id " +
                $"WHERE s.GuildId = @ServerId;", new { ServerId = serverId });
            return queryResult;
        }

        public async override Task AddAsync(AutoRole entity)
        {
            var queryResult = await QuerySingleAsync<ulong>($"INSERT INTO {TableName} (ServerId, RoleId) " +
                $"VALUES (@ServerId, @RoleId); select last_insert_rowid();",
                new { ServerId = entity.ServerId, RoleId = entity.RoleId });

            entity.Id = queryResult;
        }

        public async override Task DeleteAsync(AutoRole entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE ID = @Id;", new { Id = entity.Id });
        }

        public async Task DeleteAutoRole(ulong serverId, ulong roleId)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE ServerId = @ServerId AND roleId = @RoleId;",
                new { ServerId = serverId, RoleId = roleId });
        }

        public async override Task EditAsync(AutoRole entity)
        {
            await ExecuteAsync($"UPDATE {TableName} SET ServerId = @ServerId, RoleId=@RoleId " +
                $"WHERE Id = @Id;", entity);
        }
    }
}
