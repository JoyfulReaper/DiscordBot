using Dapper;
using DiscordBot.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DiscordBot.DataAccess
{
    public abstract class Repository<T> : IRepository<T> where T: class
    {
        private readonly string _tableName;
        private readonly Settings _settings;
        private readonly ILogger<Repository<T>> _logger;

        public Repository(Settings settings,
            ILogger<Repository<T>> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public abstract void Add(T entity);

        public abstract void Delete(T entity);

        public abstract void Edit(T entity);

        public virtual T GetById(int Id)
        {
            return QuerySingle<T>($"SELECT * FROM {_tableName} WHERE ID = @Id", new { Id = Id });
        }

        public virtual IEnumerable<T> List()
        {
            return Query<T>($"SELECT * FROM {_tableName}");
        }

        public async virtual Task<T> GetByIdAsync(int Id)
        {
            return await QuerySingleAsync<T>($"SELECT * FROM {_tableName} WHERE ID = @Id", new { Id = Id });
        }

        public async virtual Task<IEnumerable<T>> ListAsync()
        {
            return await QueryAsync<T>($"SELECT * FROM {_tableName}");
        }

        private void Execute(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    connection.Execute(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Execute Exception Thrown");
            }
        }

        private void ExecuteAsync(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    connection.ExecuteAsync(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: ExecuteAsync Exception Thrown");
            }
        }

        private IEnumerable<T> Query<T>(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    return connection.Query<T>(query, parameters).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: Query Exception Thrown");
                return default;
            }
        }

        private T QueryFirst<T>(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    return connection.QueryFirst<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: QueryFirst Exception Thrown");
                return default;
            }
        }

        private T QueryFirstOrDefault<T> (string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    return connection.QueryFirstOrDefault<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: QueryFirstOrDefault Exception Thrown");
                return default;
            }
        }

        private T QuerySingle<T>(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    return connection.QuerySingle<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: QuerySingle Exception Thrown");
                return default;
            }
        }

        private T QuerySingleOrDefault<T>(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    return connection.QuerySingleOrDefault<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: QuerySingleOrDefault Exception Thrown");
                return default;
            }
        }

        private async Task<IEnumerable<T>> QueryAsync<T>(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    var result = await connection.QueryAsync<T>(query, parameters);
                    return result.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: QueryAsync Exception Thrown");
                return default;
            }
        }

        private async Task<T> QueryFirstAsync<T>(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    return await connection.QueryFirstAsync<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: QueryFirstAsync Exception Thrown");
                return default;
            }
        }

        private async Task<T> QueryFirstOrDefaultAsync<T>(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    return await connection.QueryFirstOrDefaultAsync<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: QueryFirstOrDefaultAsync Exception Thrown");
                return default;
            }
        }

        private async Task<T> QuerySingleAsync<T>(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    return await connection.QuerySingleAsync<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: QuerySingleAsync Exception Thrown");
                return default;
            }
        }

        private async Task<T> QuerySingleOrDefaultAsync<T>(string query, object parameters = null)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_settings.ConnectionString))
                {
                    return await connection.QuerySingleOrDefaultAsync<T>(query, parameters);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Repository: QuerySingleOrDefaultAsync Exception Thrown");
                return default;
            }
        }
    }
}
