using DiscordBot.Models;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.DataAccess
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

        public async Task<List<AutoRole>> GetAutoRoleByServerId(ulong serverId)
        {
            var queryResult = await QueryAsync<AutoRole>($"SELECT * FROM {TableName} WHERE ServerId = @ServerId;", new { ServerId = serverId });
            return queryResult.ToList();
        }

        public async override Task AddAsync(AutoRole entity)
        {
            await ExecuteAsync($"INSERT INTO {TableName} (ServerId, RoleId) " +
                $"VALUES (@ServerId, @RoleId);", new { ServerId = entity.ServerId, RoleId = entity.RoleId });
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
