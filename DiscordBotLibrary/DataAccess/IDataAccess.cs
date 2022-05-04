
namespace DiscordBotLibrary.DataAccess;

public interface IDataAccess
{
    Task<int> ExecuteRawSQL<T>(string sql, T parameters);
    Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters);
    Task<IEnumerable<T>> QueryRawSQL<T, U>(string sql, U parameters);
    Task<int> SaveData<T>(string storedProcedure, T parameters);
}
