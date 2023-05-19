using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace IdentityApi.Data.DataAccess;

public interface ISqlDataAccess
{
	Task<IEnumerable<TResult>> LoadData<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task<TResult> LoadSingle<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task<TResult> LoadScalar<TResult>(string sql, DynamicParameters parameters, CancellationToken ct);
	Task SaveData(string sql, DynamicParameters parameters, CancellationToken ct);
	Task<SqlMapper.GridReader> GetMulti(string sql, DynamicParameters parameters, CancellationToken ct);
}