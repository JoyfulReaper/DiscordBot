using Dapper;
using DiscordBotLibrary.DataAccess;
using DiscordBotLibrary.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Repositories
{

    public class GuildRepository
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public GuildRepository(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DiscordBot");
        }

        public async Task UpsertGuild(Guild guild)
        {
            using var connection = new SqlConnection(_connectionString);
            var id = await connection.QuerySingleAsync<int>("spGuild_Upsert", guild, commandType: CommandType.StoredProcedure);
            guild.Id = id;
        }
    }
}
