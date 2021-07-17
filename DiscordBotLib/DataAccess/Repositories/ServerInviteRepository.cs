using DiscordBotLib.Models.DatabaseEntities;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.Repositories
{
    public class ServerInviteRepository : Repository<ServerInvite>
    {
        private readonly ISettings _settings;
        private readonly ILogger<ServerInviteRepository> _logger;

        public ServerInviteRepository(ISettings settings, ILogger<ServerInviteRepository> logger) : base (settings, logger)
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
                $"(ServerId, Uses, Code) VALUES (@ServerId, Uses, Code); " +
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
