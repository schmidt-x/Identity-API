using System.Threading;
using IdentityLibrary.Contracts.Options;
using Microsoft.Extensions.Options;

namespace IdentityLibrary.Data.DataAccess;

public class SqlDataAccess : ISqlDataAccess
{
	private readonly ConnectionStringsOptions _connStrings;
	
	public SqlDataAccess(IOptions<ConnectionStringsOptions> options)
	{
		_connStrings = options.Value;
	}
	
	private SqlConnection GetConnection() =>
		new SqlConnection(_connStrings.Mssql);
	
	
	public async Task<IEnumerable<TResult>> LoadData<TResult>(string sql, DynamicParameters parameters, CancellationToken token)
	{
		await using var cnn = GetConnection();
		
		return await cnn.QueryAsync<TResult>(new CommandDefinition(sql, parameters, cancellationToken: token));
	}
	
	public async Task<TResult> LoadSingle<TResult>(string sql, DynamicParameters parameters, CancellationToken token)
	{
		await using var cnn = GetConnection();
		
		return await cnn.QueryFirstOrDefaultAsync<TResult>(new CommandDefinition(sql, parameters, cancellationToken: token));
	}
	
	public async Task<TResult> LoadScalar<TResult>(string sql, DynamicParameters parameters, CancellationToken token)
	{
		await using var cnn = GetConnection();
		
		return await cnn.ExecuteScalarAsync<TResult>(new CommandDefinition(sql, parameters, cancellationToken: token));
	}

	public async Task SaveData(string sql, DynamicParameters parameters, CancellationToken token)
	{
		await using var cnn = GetConnection();
		
		await cnn.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: token));
	}
}