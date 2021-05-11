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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.DataAccess
{
    class SubredditRepository : Repository<Subreddit>
    {
        private readonly ILogger<SubredditRepository> _logger;
        private readonly Settings _settings;

        public SubredditRepository(ILogger<SubredditRepository> logger,
            Settings settings) : base(settings, logger)
        {
            _logger = logger;
            _settings = settings;
        }

        public async override Task AddAsync(Subreddit entity)
        {
            await ExecuteAsync($"INSERT INTO {TableName} (ServerId, Name) " +
                $"VALUES (@ServerId, @Name);", entity);
        }

        public async override Task DeleteAsync(Subreddit entity)
        {
            throw new NotImplementedException();
        }

        public async override Task EditAsync(Subreddit entity)
        {
            throw new NotImplementedException();
        }
    }
}
