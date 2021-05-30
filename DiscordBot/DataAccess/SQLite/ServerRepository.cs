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

using Dapper;
using DiscordBot.Models;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DiscordBot.DataAccess.SQLite
{
    public class ServerRepository : Repository<Server>, IServerRepository
    {
        private readonly ISettings _settings;
        private readonly ILogger<ServerRepository> _logger;

        public ServerRepository(ISettings settings,
            ILogger<ServerRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<Server> GetByServerId(ulong guildId)
        {
            //var parameters = GetDynamicParameters()
            var queryResult = await QuerySingleOrDefaultAsync<Server>($"SELECT * FROM {TableName} WHERE GuildId = @GuildId;", new { GuildId = guildId });

            //try
            //{
            //    queryResult.EmbedColor
            //}

            return queryResult;
        }

        public async override Task AddAsync(Server entity)
        {
            var parameters = GetDynamicParameters(entity);

            var queryResult = await QuerySingleAsync<ulong>($"INSERT INTO {TableName} " +
                $"(GuildId, Prefix, SubredditLearning, WelcomeChannel, WelcomeBackground, LoggingChannel, EmbedColor)" +
                "VALUES (@GuildId, @Prefix, @SubredditLearning, @WelcomeChannel, @WelcomeBackground, @LoggingChannel, @EmbedColor); " +
                "SELECT last_insert_rowid();",
                parameters);

            entity.Id = queryResult;
        }

        private DynamicParameters GetDynamicParameters(Server entity)
        {
            string color = $"{entity.EmbedColor.R},{entity.EmbedColor.G},{entity.EmbedColor.B}";

            var parameters = new DynamicParameters();
            parameters.Add("@Id", entity.Id);
            parameters.Add("@GuildId", entity.GuildId);
            parameters.Add("@Prefix", entity.Prefix);
            parameters.Add("@SubredditLearning", entity.SubredditLearning);
            parameters.Add("@WelcomeChannel", entity.WelcomeChannel);
            parameters.Add("@WelcomeBackground", entity.WelcomeBackground);
            parameters.Add("@LoggingChannel", entity.LoggingChannel);
            parameters.Add("@EmbedColor", color);

            return parameters;
        }

        public async Task AddAsync(ulong id)
        {
            var defaultPrefix = _settings.DefaultPrefix;
            await AddAsync(new Server { GuildId = id, Prefix = defaultPrefix });
        }

        public async override Task DeleteAsync(Server entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE Id=@Id;", new { Id = entity.Id });
        }

        public async override Task EditAsync(Server entity)
        {
            var parameters = GetDynamicParameters(entity);

            await ExecuteAsync($"UPDATE {TableName} " +
                $"SET Prefix = @Prefix, GuildId = @GuildId, SubredditLearning = @SubredditLearning, WelcomeChannel = @WelcomeChannel, WelcomeBackground = @WelcomeBackground, " +
                $"EmbedColor = @EmbedColor, LoggingChannel = @LoggingChannel " +
                $"WHERE Id = @Id;",
                parameters);
        }
    }
}
