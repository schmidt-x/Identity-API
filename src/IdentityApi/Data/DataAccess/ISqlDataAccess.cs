using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace IdentityApi.Data.DataAccess;

public interface ISqlDataAccess
{
	Task<IEnumerable<TResult>> QueryAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task<TResult> QuerySingleAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task<TResult?> QuerySingleOrDefaultAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task<TResult> QueryScalarAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task ExecuteAsync(string sql, DynamicParameters parameters, CancellationToken ct);
}