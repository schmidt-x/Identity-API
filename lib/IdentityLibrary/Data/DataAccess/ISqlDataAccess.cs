using System.Threading;

namespace IdentityLibrary.Data.DataAccess;

public interface ISqlDataAccess
{
	Task<IEnumerable<TResult>> LoadData<TResult>(string sql, DynamicParameters parameters, CancellationToken token);
	Task<TResult> LoadSingle<TResult>(string sql, DynamicParameters parameters, CancellationToken token);
	Task<TResult> LoadScalar<TResult>(string sql, DynamicParameters parameters, CancellationToken token);
	Task SaveData(string sql, DynamicParameters parameters, CancellationToken token);
}