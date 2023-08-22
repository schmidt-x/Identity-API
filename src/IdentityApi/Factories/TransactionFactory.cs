using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace IdentityApi.Factories;

public class TransactionFactory : IDisposable, IAsyncDisposable
{
	private readonly SqlConnection _connection;
	private SqlTransaction? _transaction;
	
	public bool IsActive => _transaction?.Connection is not null;
	
	public TransactionFactory(SqlConnection connection)
	{
		_connection = connection;
	}
	
	
	public async Task<SqlTransaction> GetTransactionAsync()
	{
		if (_transaction is null)
		{
			if (_connection.State is ConnectionState.Closed)
				await _connection.OpenAsync();
			
			_transaction = (SqlTransaction) await _connection.BeginTransactionAsync();
		}

		else if (_transaction.Connection is null) // true if commit has been invoked
		{
			await _transaction.DisposeAsync();
			
			_transaction = (SqlTransaction) await _connection.BeginTransactionAsync();
		}

		return _transaction;
	}
	
	public SqlTransaction GetTransaction()
	{
		if (_transaction is null)
		{
			if (_connection.State is ConnectionState.Closed)
				_connection.Open();
			
			_transaction = _connection.BeginTransaction();
		}
		
		else if (_transaction.Connection is null)
		{
			_transaction.Dispose();
			
			_transaction = _connection.BeginTransaction();
		}
		
		return _transaction;
	}
	
	
	public async ValueTask DisposeAsync()
	{
		if (_transaction is not null)
			await _transaction.DisposeAsync();
		
		await _connection.DisposeAsync();
	}
	
	public void Dispose()
	{
		_transaction?.Dispose();
		_connection.Dispose();
	}
}