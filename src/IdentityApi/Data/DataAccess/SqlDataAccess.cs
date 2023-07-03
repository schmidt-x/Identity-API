using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IdentityApi.Contracts.Options;
using Microsoft.Extensions.Options;

namespace IdentityApi.Data.DataAccess;

public class SqlDataAccess : ISqlDataAccess
{
	private readonly ConnectionStringsOptions _connStrings;
	
	public SqlDataAccess(IOptions<ConnectionStringsOptions> options)
	{
		_connStrings = options.Value;
	}
	
	private SqlConnection GetConnection() =>
		new SqlConnection(_connStrings.Mssql);
	
	
	public async Task<IEnumerable<TResult>> LoadData<TResult>(string sql, DynamicParameters parameters, CancellationToken ct = default)
	{
		await using var cnn = GetConnection();
		
		return await cnn.QueryAsync<TResult>(new CommandDefinition(sql, parameters, cancellationToken: ct));
	}
	
	public async Task<TResult> LoadSingle<TResult>(string sql, DynamicParameters parameters, CancellationToken ct = default)
	{
		await using var cnn = GetConnection();
		
		return await cnn.QuerySingleAsync<TResult>(new CommandDefinition(sql, parameters, cancellationToken: ct));
	}
	
	public async Task<TResult?> LoadSingleOrDefault<TResult>(string sql, DynamicParameters parameters, CancellationToken ct = default)
	{
		await using var cnn = GetConnection();
		
		return await cnn.QuerySingleOrDefaultAsync<TResult>(new CommandDefinition(sql, parameters, cancellationToken: ct));
	}
	
	public async Task<TResult> LoadScalar<TResult>(string sql, DynamicParameters parameters, CancellationToken ct = default)
	{
		await using var cnn = GetConnection();
		
		return await cnn.ExecuteScalarAsync<TResult>(new CommandDefinition(sql, parameters, cancellationToken: ct));
	}

	public async Task SaveData(string sql, DynamicParameters parameters, CancellationToken ct = default)
	{
		await using var cnn = GetConnection();
		
		await cnn.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: ct));
	}

	public async Task<TResult> SaveData<TResult>(string sql, DynamicParameters parameters, CancellationToken ct = default)
	{
		await using var cnn = GetConnection();
		
		return await cnn.QuerySingleAsync<TResult>(new CommandDefinition(sql, parameters, cancellationToken: ct));
	}
}