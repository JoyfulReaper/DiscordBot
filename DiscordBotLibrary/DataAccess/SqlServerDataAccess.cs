// Inspired by some Tim Cory code
// I don't think I will actually use it though...

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace DiscordBotLibrary.DataAccess
{
    public class SqlServerDataAccess : IDataAccess
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public SqlServerDataAccess(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DiscordBot");
        }

        /// <summary>
        /// Load data from the database
        /// </summary>
        /// <typeparam name="T">Type of the data to retrieve</typeparam>
        /// <param name="storedProcedure">The store procedure to execute</param>
        /// <param name="parameters">Paramaters for the stored procedure</param>
        /// <returns>A list of type T</returns>
        public Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var rows = connection.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
                return rows;
            }
        }

        /// <summary>
        /// Save data to the database
        /// </summary>
        /// <param name="storedProcedure">The stored procedure to execute</param>
        /// <param name="parameters">The paremeters for the store procedure</param>
        /// <returns>The number of rows affected</returns>
        public Task<int> SaveData<T>(string storedProcedure, T parameters)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                return connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
            }
        }

        /// <summary>
        /// Save data to the database
        /// </summary>
        /// <param name="storedProcedure">The stored procedure to execute</param>
        /// <param name="parameters">The paremeters for the store procedure</param>
        /// <returns>The number of rows affected</returns>
        public async Task<int> SaveDataReturnsId<T>(string storedProcedure, T parameters)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var id =  await connection.QuerySingleAsync<int>(storedProcedure, parameters, commandType: CommandType.StoredProcedure);
                return id;
            }
        }        

        public Task<IEnumerable<T>> QueryRawSQL<T, U>(string sql, U parameters)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var res = connection.QueryAsync<T>(sql, parameters);
                return res;
            }
        }

        public Task<int> ExecuteRawSQL<T>(string sql, T parameters)
        {
            using (IDbConnection connection = new SqlConnection(_connectionString))
            {
                var res = connection.ExecuteAsync(sql, parameters);
                return res;
            }
        }
    }
}
