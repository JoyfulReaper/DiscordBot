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
using System.Threading.Tasks;
using System;
using System.Linq;

namespace DiscordBotLib.DataAccess.Repositories
{
    public class DiscordBotSettingsRepository : Repository<DiscordBotSettings>, IDiscordBotSettingsRepository
    {
        private readonly ILogger<DiscordBotSettingsRepository> _logger;
        private readonly ISettings _settings;

        public DiscordBotSettingsRepository(ILogger<DiscordBotSettingsRepository> logger,
            ISettings settings) : base(settings, logger)
        {
            _logger = logger;
            _settings = settings;
        }

        public async Task<DiscordBotSettings> Get()
        {
            var queryResult = await QuerySingleAsync<int>($"SELECT COUNT (Token) " +
                $"FROM {TableName};");

            if (queryResult > 1)
            {
                _logger.LogCritical("Multiple settings rows exist in the database!");
                throw new InvalidOperationException("Multiple settings rows exist in the database!"); ;
            }

            var output = await QuerySingleOrDefaultAsync<DiscordBotSettings>($"SELECT * FROM {TableName};");
            return output;
        }

        public override async Task AddAsync(DiscordBotSettings entity)
        {
            var queryResult = await QuerySingleAsync<int>($"SELECT COUNT (Token) " +
                $"FROM {TableName};");

            if (queryResult > 0)
            {
                _logger.LogError("Settings already exist in the database!");
                return;
            }

            var id = await QuerySingleAsync<ulong>($"INSERT INTO {TableName} (Token, Game) " +
                $"VALUES (@Token, @Game); select last_insert_rowid();",
                entity);

            entity.Id = id;
        }

        public override async Task DeleteAsync(DiscordBotSettings entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE Id = @Id",
                new { Id = entity.Id });
        }

        public override async Task EditAsync(DiscordBotSettings entity)
        {
            await ExecuteAsync($"UPDATE {TableName} " +
                $"SET Token = @Token, Game = @Game " +
                $"WHERE Id = @Id",
                entity);
        }
    }
}
