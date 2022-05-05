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

public class GuildRepository : IGuildRepository
{
    private readonly IConfiguration _config;
    private readonly string _connectionString;

    public GuildRepository(IConfiguration config)
    {
        _config = config;
        _connectionString = _config.GetConnectionString("DiscordBot");
    }
    
    public async Task SaveGuildAsync(Guild guild)
    {
        using var connection = new SqlConnection(_connectionString);
        var id = await connection.QuerySingleAsync<long>("spGuild_Upsert", guild, commandType: CommandType.StoredProcedure);
        guild.GuildId = id;
    }

    public async Task<Guild> LoadGuildAsync(string discordGuildId)
    {
        using var connection = new SqlConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<Guild>("spGuild_Load", new { DiscordGuildId = discordGuildId }, commandType: CommandType.StoredProcedure);
    }
}