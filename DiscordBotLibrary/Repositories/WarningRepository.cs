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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotLibrary.Repositories;

public class WarningRepository : IWarningRepository
{
    private readonly IConfiguration _config;
    private readonly string _connectionString;


    public WarningRepository(IConfiguration config)
    {
        _config = config;
        _connectionString = _config.GetConnectionString("DiscordBot");
    }

    public async Task ClearWarningsAsync(long userId, long guildId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("spWarning_Clear", new { userId, guildId }, commandType: CommandType.StoredProcedure);

    }

    public async Task<IEnumerable<Warning>> GetWarningsAsync(long userId, long guildId)
    {
        using var connection = new SqlConnection(_connectionString);
        var warnings = await connection.QueryAsync<Warning>("spWarning_Get", new { userId, guildId }, commandType: CommandType.StoredProcedure);
        return warnings;
    }

    public async Task AddWarningAsync(Warning warning)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.ExecuteAsync("spWarning_Add", new
        {
            UserId = warning.UserId,
            GuildId = warning.GuildId,
            Text = warning.Text
        }, commandType: CommandType.StoredProcedure);
    }

    public async Task<WarningAction?> GetWarningActionAsync(long guildId)
    {
        using var connection = new SqlConnection(_connectionString);
        var wAction = await connection.QuerySingleOrDefaultAsync<WarningAction>("spWarningAction_Get", new { guildId }, commandType: CommandType.StoredProcedure);
        return wAction;
    }

    public async Task SetWarningActionAsync(WarningAction action)
    {
        using var connection = new SqlConnection(_connectionString);
        var warnActionId = await connection.QuerySingleAsync<long>("spWarningAction_Upsert", new
        {
            WarningActionId = action.WarningActionId,
            GuildId = action.GuildId,
            Action = action.Action,
            ActionThreshold = action.ActionThreshold
        },
            commandType: CommandType.StoredProcedure);
        action.WarningActionId = warnActionId;
    }
}
