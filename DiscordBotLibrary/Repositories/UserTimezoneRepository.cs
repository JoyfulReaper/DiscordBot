/*
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

namespace DiscordBotLibrary.Repositories;
public class UserTimezoneRepository : IUserTimezoneRepository
{
    private readonly IConfiguration _config;

    public UserTimezoneRepository(IConfiguration config)
    {
        _config = config;
    }

    public async Task SaveUserTimezoneAsync(UserTimezone userTimezone)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("DiscordBot"));
        var id = await connection.QuerySingleAsync<long>("spUserTimezone_Upsert",
            new
            {
                UserTimezoneId = userTimezone.UserTimezoneId,
                UserId = userTimezone.UserId,
                Timezone = userTimezone.TimeZone,
            }, commandType: System.Data.CommandType.StoredProcedure);
        userTimezone.UserTimezoneId = id;
    }
    
    public async Task<UserTimezone> LoadUserTimeZoneAsync(long userId)
    {
        using var connection = new SqlConnection(_config.GetConnectionString("DiscordBot"));
        return await connection.QuerySingleOrDefaultAsync<UserTimezone>("spUserTimezone_Load", new
        {
            UserId = userId
        }, commandType: System.Data.CommandType.StoredProcedure);
    }
}
