﻿/*
MIT License

Copyright(c) 2022 Kyle Givler
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
using DiscordBotLibrary.Models;
using DiscordBotLibrary.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace DiscordBotLibrary.Repositories;
public class NoteRepository : INoteRepository
{
    private readonly IConfiguration _config;
    private readonly string _connectionString;

    public NoteRepository(IConfiguration config)
    {
        _config = config;
        _connectionString = _config.GetConnectionString("DiscordBot");
    }

    public async Task SaveNoteAsync(Note note)
    {
        using var connection = new SqlConnection(_connectionString);
        var id = await connection.QuerySingleAsync<long>("spNote_Upsert", new
        {
            NoteId = note.NoteId,
            UserId = note.UserId,
            Name = note.Name,
            Text = note.Text
        }, commandType: CommandType.StoredProcedure);

        note.NoteId = id;
    }

    public async Task<IEnumerable<Note>> GetNotesAsync(long userId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QueryAsync<Note>("spNote_LoadAll", new { UserId = userId }, commandType: CommandType.StoredProcedure);
    }

    public Task DeleteNoteAsync(long noteId)
    {
        using var connection = new SqlConnection(_connectionString);
        return connection.ExecuteAsync("spNote_Delete", new { NoteId = noteId }, commandType: CommandType.StoredProcedure);
    }
}