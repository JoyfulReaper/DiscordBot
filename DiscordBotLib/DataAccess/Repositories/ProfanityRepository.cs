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

using DiscordBotLib.Enums;
using DiscordBotLib.Models;
using DiscordBotLib.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.Repositories
{
    public class ProfanityRepository : Repository<Profanity>, IProfanityRepository
    {
        // TODO take the duplicated code from getting the server and make it into a pricate method

        private readonly ILogger<ProfanityRepository> _logger;
        private readonly ISettings _settings;
        private readonly IServerRepository _serverRepository;

        public ProfanityRepository(ILogger<ProfanityRepository> logger,
            ISettings settings,
            IServerRepository serverRepository) : base(settings, logger)
        {
            _logger = logger;
            _settings = settings; _serverRepository = serverRepository;
            ;
        }

        public async Task AllowProfanity(ulong serverId, string profanity)
        {
            var profanityDB = await GetProfanity(profanity);
            if (profanityDB == null)
            {
                profanityDB = new Profanity { Word = profanity };
                await AddAsync(profanityDB);
            }

            var server = await _serverRepository.GetByServerId(serverId);
            if(server == null)
            {
                _logger.LogWarning("Attempted to access a server ({server}) that does not exist.", serverId);
                throw new ArgumentException("Server does not exist!", nameof(serverId));
            }

            int count = await QueryFirstOrDefaultAsync<int>($"SELECT count(Id) " +
                $"FROM ProfanityServer WHERE ServerId = @ServerId " +
                $"AND ProfanityId = @ProfanityId;",
                new { ServerId = server.Id, ProfanityId = profanityDB.Id});

            if (count != 0)
            {
                await ExecuteAsync($"UPDATE ProfanityServer " +
                     $"SET ServerId = @ServerId, ProfanityId = @ProfanityId, ProfanityMode = @ProfanityMode " +
                     $"WHERE ServerId = @ServerId AND ProfanityId = @ProfanityId;",
                     new { ServerId = server.Id, ProfanityId = profanityDB.Id, ProfanityMode = (int)ProfanityMode.Allow });
            }
            else
            {
                await ExecuteAsync($"INSERT INTO ProfanityServer " +
                    $"(ServerId, ProfanityId, ProfanityMode) " +
                    $"VALUES (@ServerId, @ProfanityId, @ProfanityMode);",
                    new { ServerId = server.Id, ProfanityId = profanityDB.Id, ProfanityMode = (int)ProfanityMode.Allow });
            }
        }

        public async Task BlockProfanity(ulong serverId, string profanity)
        {
            var profanityDB = await GetProfanity(profanity);
            if (profanityDB == null)
            {
                profanityDB = new Profanity { Word = profanity };
                await AddAsync(profanityDB);
            }

            var server = await _serverRepository.GetByServerId(serverId);
            if (server == null)
            {
                _logger.LogWarning("Attempted to access a server ({server}) that does not exist.", serverId);
                throw new ArgumentException("Server does not exist!", nameof(serverId));
            }

            int count = await QueryFirstOrDefaultAsync<int>($"SELECT count(Id) " +
                $"FROM ProfanityServer WHERE ServerId = @ServerId " +
                $"AND ProfanityId = @ProfanityId;",
                new { ServerId = server.Id, ProfanityId = profanityDB.Id});

            if (count != 0)
            {
                await ExecuteAsync($"UPDATE ProfanityServer " +
                 $"SET ServerId = @ServerId, ProfanityId = @ProfanityId, ProfanityMode = @ProfanityMode " +
                 $"WHERE ServerId = @ServerId;",
                 new { ServerId = server.Id, ProfanityId = profanityDB.Id, ProfanityMode = (int)ProfanityMode.Block });
            }
            else
            {
                await ExecuteAsync($"INSERT INTO ProfanityServer " +
                    $"(ServerId, ProfanityId, ProfanityMode) " +
                    $"VALUES (@ServerId, @ProfanityId, @ProfanityMode);",
                    new { ServerId = server.Id, ProfanityId = profanityDB.Id, ProfanityMode = (int)ProfanityMode.Block });
            }
        }

        public async Task<List<Profanity>> GetAllowedProfanity(ulong serverId)
        {
            var server = await _serverRepository.GetByServerId(serverId);
            if (server == null)
            {
                _logger.LogWarning("Attempted to access a server ({server}) that does not exist.", serverId);
                throw new ArgumentException("Server does not exist!", nameof(serverId));
            }

            var QueryResult = await QueryAsync<Profanity>($"SELECT p.Id, p.Word " +
                $"FROM Profanity p " +
                $"INNER JOIN ProfanityServer ps on ps.ProfanityId = p.Id " +
                $"INNER JOIN Server s on s.Id = ps.ServerId " +
                $"WHERE ServerId = @ServerId and ps.ProfanityMode = {(int)ProfanityMode.Allow};",
                new { ServerId = server.Id });

            return QueryResult.ToList();
        }

        public async Task<List<Profanity>> GetBlockedProfanity(ulong serverId)
        {
            var server = await _serverRepository.GetByServerId(serverId);
            if (server == null)
            {
                _logger.LogWarning("Attempted to access a server ({server}) that does not exist.", serverId);
                throw new ArgumentException("Server does not exist!", nameof(serverId));
            }

            var QueryResult = await QueryAsync<Profanity>($"SELECT p.Id, Word " +
                $"FROM Profanity p " +
                $"INNER JOIN ProfanityServer ps on ps.ProfanityId = p.Id " +
                $"INNER JOIN Server s on s.Id = ps.ServerId " +
                $"WHERE ServerId = @ServerId and ps.ProfanityMode = {(int)ProfanityMode.Block};",
                new { ServerId = server.Id });

            return QueryResult.ToList();
        }

        public async override Task AddAsync(Profanity entity)
        {
            var queryResult = await QuerySingleAsync<ulong>($"INSERT INTO {TableName} (Word) " +
                $"VALUES (@Word); select last_insert_rowid();", entity);

            entity.Id = queryResult;
        }

        public async Task<Profanity> AddAsync(ulong serverId, string profanity, ProfanityMode mode)
        {
            var server = await _serverRepository.GetByServerId(serverId);
            if (server == null)
            {
                _logger.LogWarning("Attempted to access a server ({server}) that does not exist.", serverId);
                throw new ArgumentException("Server does not exist!", nameof(serverId));
            }

            var profanityObj = await GetProfanity(profanity);
            if (profanityObj == null)
            {
                profanityObj = new Profanity { Word = profanity };
                await AddAsync(profanityObj);
            }

            await ExecuteAsync($"INSERT INTO ProfanityServer " +
                $"(ServerId, ProfanityId, Mode) " +
                $"VALUES (@ServerId, @ProfanityId, @Mode);",
                new { ServerId = server.Id, ProfanityId = profanityObj.Id, Mode = mode });

            return profanityObj;
        }

        public async override Task DeleteAsync(Profanity entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE Id = @Id;",
                new { Id = entity.Id });
        }

        public async Task DeleteAsync(ulong serverId, ulong profanity)
        {
            var server = await _serverRepository.GetByServerId(serverId);
            if (server == null)
            {
                _logger.LogWarning("Attempted to access a server ({server}) that does not exist.", serverId);
                throw new ArgumentException("Server does not exist!", nameof(serverId));
            }

            await ExecuteAsync($"DELETE FROM ProfanityServer " +
                $"WHERE ServerId = @ServerId AND ProfanityId = @ProfanityId;",
                new { ServerId = server.Id, ProfanityId = profanity });
        }

        public async Task DeleteAsync(string profanity)
        {
            var profanityObj = GetProfanity(profanity);
            if (profanityObj != null)
            {
                await ExecuteAsync($"DELETE FROM ProfanityServer " +
                    $"WHERE ProfanityId = @ProfanityId;",
                    new { ProfanityId = profanityObj.Id });
            }

            await ExecuteAsync($"DELETE FROM Profanity " +
                $"WHERE Word = @Word",
                new { Word = profanity });
        }

        public async override Task EditAsync(Profanity entity)
        {
            await ExecuteAsync($"UPDATE {TableName} SET Word = @Word,  " +
                $"WHERE Id = @Id;", entity);
        }

        public async Task<Profanity> GetProfanity(string word)
        {
            var queryResult = await QueryFirstOrDefaultAsync<Profanity>($"SELECT * " +
                $"FROM {TableName} WHERE @Word = Word;",
                new { Word = word });

            return queryResult;
        }
    }
}
