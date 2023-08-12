using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace IdentityApi.Data.DataAccess;

public interface ISqlDataAccess
{
	Task<IEnumerable<TResult>> LoadAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task<TResult> LoadSingleAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task<TResult?> LoadSingleOrDefaultAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task<TResult> LoadScalarAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task ExecuteAsync(string sql, DynamicParameters parameters, CancellationToken ct);
}