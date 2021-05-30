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

using DiscordBot.Models;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBot.DataAccess.SQLite
{
    public class SubredditRepository : Repository<Subreddit>, ISubredditRepository
    {
        private readonly ILogger<SubredditRepository> _logger;
        private readonly ISettings _settings;

        public SubredditRepository(ILogger<SubredditRepository> logger,
            ISettings settings) : base(settings, logger)
        {
            _logger = logger;
            _settings = settings;
        }

        public async Task<List<Subreddit>> GetSubredditListByServerId(ulong guildId)
        {
            var queryResult = await QueryAsync<Subreddit>($"SELECT r.Id, Name " +
                $"FROM Subreddit r " +
                $"INNER JOIN ServerSubreddit ss on ss.SubredditId = r.Id " +
                $"INNER JOIN Server s on s.GuildId = ss.ServerId " +
                $"WHERE ServerId = @GuildId",
                new { GuildId = guildId });

            return queryResult.ToList();
        }

        public async Task<Subreddit> GetSubredditByServerId(ulong guildId, string subreddit)
        {
            var queryResult = await QueryFirstOrDefaultAsync<Subreddit>($"SELECT r.Id, Name " +
                $"FROM Subreddit r " +
                $"INNER JOIN ServerSubreddit ss on ss.SubredditId = r.Id " +
                $"INNER JOIN Server s on s.GuildId = ss.ServerId " +
                $"WHERE ServerId = @GuildId " +
                $"AND Name = @Name",
                new { GuildId = guildId, Name = subreddit });

            return queryResult;
        }

        public async Task<Subreddit> GetSubreddit(string name)
        {
            var queryResult = await QueryFirstOrDefaultAsync<Subreddit>($"SELECT * " +
                $"FROM {TableName} WHERE @Name = Name;",
                new { Name = name });

            return queryResult;
        }

        public async override Task AddAsync(Subreddit entity)
        {
            var queryResult = await QuerySingleAsync<ulong>($"INSERT INTO {TableName} (Name) " +
                $"VALUES (@Name); select last_insert_rowid();", entity);

            entity.Id = queryResult;
        }

        public async Task<Subreddit> AddAsync(ulong serverId, string subreddit)
        {
            var sub = await GetSubreddit(subreddit);
            if (sub == null)
            {
                sub = new Subreddit { Name = subreddit };
                await AddAsync(sub);
            }

            await ExecuteAsync($"INSERT INTO ServerSubreddit " +
                $"(ServerId, SubredditId) " +
                $"VALUES (@ServerId, @SubredditId);",
                new { ServerId = serverId, SubredditId = sub.Id });

            return sub;
        }

        public async override Task DeleteAsync(Subreddit entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE Id = @Id;",
                new { Id = entity.Id });
        }

        public async Task DeleteAsync(ulong serverId, ulong subreddit)
        {
            await ExecuteAsync($"DELETE FROM ServerSubreddit " +
                $"WHERE ServerId = @ServerId AND SubredditId = @SubredditId;",
                new { ServerId = serverId, SubredditId = subreddit });
        }

        public async Task DeleteAsync(string subreddit)
        {
            var sub = GetSubreddit(subreddit);
            if (sub != null)
            {
                await ExecuteAsync($"DELETE FROM ServerSubreddit " +
                    $"WHERE SubredditId = @SubredditId;",
                    new { SubredditId = sub.Id });
            }

            await ExecuteAsync($"DELETE FROM Subreddit " +
                $"WHERE Name = @Name",
                new { Name = subreddit });
        }

        public async override Task EditAsync(Subreddit entity)
        {
            await ExecuteAsync($"UPDATE {TableName} SET Name = @Name " +
                $"WHERE Id = @Id;", entity);
        }
    }
}
