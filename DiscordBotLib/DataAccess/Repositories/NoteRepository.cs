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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLib.DataAccess.Repositories
{
    public class NoteRepository : Repository<Note>, INoteRepository
    {
        private readonly ISettings _settings;
        private readonly ILogger<NoteRepository> _logger;

        public NoteRepository(ISettings settings,
            ILogger<NoteRepository> logger) : base(settings, logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public async Task<IEnumerable<Note>> GetNotesByUserId(ulong userId)
        {
            var queryResult = await QueryAsync<Note>("SELECT n.Id, n.Text, n.UserId, n.Name " +
                "FROM Note n " +
                "INNER JOIN User u On n.UserId = u.Id " +
                "WHERE u.UserId = @UserId;",
                new { UserId = userId });

            return queryResult;
        }

        //public async Task AddAsync(Note entity, User user)
        //{
        //    var userDb = await QuerySingleOrDefaultAsync<User>("SELECT * FROM User WHERE UserId = @UserId", new { UserId = user.UserId });
        //    if (userDb == null)
        //    {
        //        _logger.LogWarning("User ({user}) does not exist!", user.UserName);
        //        throw new ArgumentException("User does not exist.", nameof(user));
        //    }

        //    var queryResult = await QuerySingleAsync<ulong>($"INSERT INTO {TableName} (Text, Name, UserId) " +
        //        $"VALUES (@Text, @Name, @UserId); select last_insert_rowid();", 
        //        new { Text = entity.Text, Name = entity.Name, UserId = user.UserId });
        //    entity.Id = queryResult;
        //}

        public override async Task AddAsync(Note entity)
        {
            //var queryResult = await QuerySingleAsync<ulong>($"INSERT INTO {TableName} (Text, Name) " +
            //    $"VALUES (@Text, @Name); select last_insert_rowid();", entity);

            //entity.Id = queryResult;

            var queryResult = await QuerySingleAsync<ulong>($"INSERT INTO {TableName} (Text, Name, UserId) " +
                $"VALUES (@Text, @Name, @UserId); select last_insert_rowid();",
                entity);

            entity.Id = queryResult;
        }

        //public async Task RemoveAsync(int noteId, ulong userId)
        //{
        //    var queryResult = await QuerySingleOrDefaultAsync<int>("SELECT COUNT(Text) FROM Note n " +
        //        "WHERE NoteId = @NoteId", new { NoteId = noteId });

        //    if (queryResult < 1)
        //    {
        //        throw new ArgumentException("Note does not exist", nameof(noteId));
        //    }

        //    queryResult = await QuerySingleOrDefaultAsync<int>("SELECT COUNT(Text) FROM Note n " +
        //         "INNER JOIN UserNote un on un.NoteId = n.id" +
        //         "INNER JOIN User u on u.UserId" +
        //         "WHERE u.UserId = @UserId", new { UserId = userId });

        //    if (queryResult < 1)
        //    {
        //        throw new ArgumentException("User does not have any notes!", nameof(userId));
        //    }

        //    await ExecuteAsync("DELETE FROM UserNote WHERE NoteId @NoteId AND UserId = @UserId", new { NoteId = noteId, UserId = userId });
        //}

        public override async Task DeleteAsync(Note entity)
        {
            await ExecuteAsync($"DELETE FROM {TableName} WHERE ID = @Id;", entity);
        }

        public override async Task EditAsync(Note entity)
        {
            await ExecuteAsync($"UPDATE {TableName} SET Text = @Text, Name = @Name, UserId = @UserId " +
                $"WHERE Id = @Id", entity);
        }
    }
}
