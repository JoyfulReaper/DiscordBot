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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.Repositories
{
    public class SubredditRepository : Repository<Subreddit>, ISubredditRepository
    {
        private readonly ILogger<SubredditRepository> _logger;
        private readonly ISettings _settings;
        private readonly IServerRepository _serverRepository;

        public SubredditRepository(ILogger<SubredditRepository> logger,
            ISettings settings,
            IServerRepository serverRepository) : base(settings, logger)
        {
            _logger = logger;
            _settings = settings;
            _serverRepository = serverRepository;
        }

        private async Task<Server> GetServerOrThrow(ulong serverId)
        {
            var server = await _serverRepository.GetByServerId(serverId);
            if (server == null)
            {
                _logger.LogWarning("Attempted to access a server ({server}) that does not exist.", serverId);
                throw new ArgumentException("Server does not exist!", nameof(serverId));
            }

            return server;
        }

        public async Task EnableSubredditLearning(ulong guildId)
        {
            var server = await GetServerOrThrow(guildId);

            var queryResult = await QueryFirstAsync<int>($"SELECT COUNT (Id) FROM ServerReddit WHERE ID = @Id;", new { Id = guildId });
            if (queryResult == 0)
            {
                await ExecuteAsync("INSERT INTO ServerReddit (SubredditLearning, ServerId) VALUES (1, @ServerId);", new { ServerId = server.Id });
            }
            else
            {
                await ExecuteAsync("UPDATE ServerReddit SET SubredditLearning = 1, ServerId = @ServerId;", new { ServerId = server.Id });
            }
        }

        public async Task DisableSubredditLearning(ulong guildId)
        {
            var server = await GetServerOrThrow(guildId);

            var queryResult = await QueryFirstAsync<int>($"SELECT COUNT (Id) FROM ServerReddit WHERE ID = @Id;", new { Id = server.Id });
            if (queryResult == 0)
            {
                await ExecuteAsync("INSERT INTO ServerReddit (SubredditLearning, ServerId) VALUES (0, @ServerId);", new { ServerId = server.Id });
            }
            else
            {
                await ExecuteAsync("UPDATE ServerReddit SET SubredditLearning = 0, ServerId = @ServerId;", new { ServerId = server.Id });
            }
        }

        public async Task<bool> IsSubredditLearningEnabled(ulong guildId)
        {
            var server = await GetServerOrThrow(guildId);

            var queryResult = await QueryFirstAsync<bool>($"SELECT SubredditLearning FROM ServerReddit WHERE ServerId = @ServerId;", new { ServerId = server.Id });
            return queryResult;
        }

        public async Task<IEnumerable<Subreddit>> GetSubredditListByServerId(ulong guildId)
        {
            var server = await GetServerOrThrow(guildId);

            var queryResult = await QueryAsync<Subreddit>($"SELECT r.Id, r.Name " +
                $"FROM Subreddit r " +
                $"INNER JOIN ServerSubreddit ss on ss.SubredditId = r.Id " +
                $"INNER JOIN Server s on s.Id = ss.ServerId " +
                $"WHERE ServerId = @ServerId",
                new { ServerId = server.Id });

            return queryResult;
        }

        public async Task<Subreddit> GetSubredditByServerId(ulong guildId, string subreddit)
        {
            var server = await GetServerOrThrow(guildId);

            var queryResult = await QueryFirstOrDefaultAsync<Subreddit>($"SELECT r.Id, r.Name " +
                $"FROM Subreddit r " +
                $"INNER JOIN ServerSubreddit ss on ss.SubredditId = r.Id " +
                $"INNER JOIN Server s on s.Id = ss.ServerId " +
                $"WHERE ServerId = @ServerId " +
                $"AND Name = @Name",
                new { ServerId = server.Id, Name = subreddit });

            return queryResult;
        }

        public async Task<Subreddit> GetSubreddit(string name)
        {
            var queryResult = await QueryFirstOrDefaultAsync<Subreddit>($"SELECT * " +
                $"FROM Subreddit WHERE @Name = Name;",
                new { Name = name });

            return queryResult;
        }

        public async override Task AddAsync(Subreddit entity)
        {
            var queryResult = await QuerySingleAsync<ulong>($"INSERT INTO Subreddit (Name) " +
                $"VALUES (@Name); select last_insert_rowid();", entity);

            entity.Id = queryResult;
        }

        public async Task<Subreddit> AddAsync(ulong serverId, string subreddit)
        {
            var server = await GetServerOrThrow(serverId);

            var sub = await GetSubreddit(subreddit);
            if (sub == null)
            {
                sub = new Subreddit { Name = subreddit };
                await AddAsync(sub);
            }

            await ExecuteAsync($"INSERT INTO ServerSubreddit " +
                $"(ServerId, SubredditId) " +
                $"VALUES (@ServerId, @SubredditId);",
                new { ServerId = server.Id, SubredditId = sub.Id });

            return sub;
        }

        public async override Task DeleteAsync(Subreddit entity)
        {
            await ExecuteAsync($"DELETE FROM Subreddit WHERE Id = @Id;",
                new { Id = entity.Id });
        }

        public async Task DeleteAsync(ulong serverId, ulong subreddit)
        {
            var server = await GetServerOrThrow(serverId);

            await ExecuteAsync($"DELETE FROM ServerSubreddit " +
                $"WHERE ServerId = @ServerId AND SubredditId = @SubredditId;",
                new { ServerId = server.Id, SubredditId = subreddit });
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
            await ExecuteAsync($"UPDATE Subreddit SET Name = @Name " +
                $"WHERE Id = @Id;", entity);
        }
    }
}
