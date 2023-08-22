using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IdentityApi.Factories;

namespace IdentityApi.Data.DataAccess;

public class SqlDataAccess : ISqlDataAccess
{
	private readonly SqlConnection _cnn;
	private readonly TransactionFactory _transactionFactory;
	
	public SqlDataAccess(TransactionFactory transactionFactory, SqlConnection cnn)
	{
		_transactionFactory = transactionFactory;
		_cnn = cnn;
	}
	
	private Task<SqlTransaction> GetTransactionAsync() => _transactionFactory.GetTransactionAsync();
	
	
	public async Task<IEnumerable<TResult>> LoadAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct)
	{
		var transaction = await GetTransactionAsync();
		
		return await _cnn.QueryAsync<TResult>(new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));
	}
	
	public async Task<TResult> LoadSingleAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct)
	{
		var transaction = await GetTransactionAsync();
		
		return await _cnn.QuerySingleAsync<TResult>(new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));
	}
	
	public async Task<TResult?> LoadSingleOrDefaultAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct)
	{
		var transaction = await GetTransactionAsync();
		
		return await _cnn.QuerySingleOrDefaultAsync<TResult>(new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));
	}
	
	public async Task<TResult> LoadScalarAsync<TResult>(string sql, DynamicParameters parameters, CancellationToken ct)
	{
		var transaction = await GetTransactionAsync();
		
		return await _cnn.ExecuteScalarAsync<TResult>(new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));
	}

	public async Task ExecuteAsync(string sql, DynamicParameters parameters, CancellationToken ct)
	{
		var transaction = await GetTransactionAsync();
		
		await _cnn.ExecuteAsync(new CommandDefinition(sql, parameters, transaction, cancellationToken: ct));
	}
	
}