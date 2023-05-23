using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IdentityApi.Data.DataAccess;
using IdentityApi.Enums;
using IdentityApi.Models;
using IdentityApi.Results;

namespace IdentityApi.Data.Repositories;

public class UserRepository : IUserRepository
{
	private readonly ISqlDataAccess _db;
	
	public UserRepository(ISqlDataAccess db)
	{
		_db = db;
	}
	
	
	public async Task<bool> ExistsAsync<T>(T arg, Column column, CancellationToken ct = default)
	{
		var argName = GetColumnName(column);
		
		var sql = $"""
			SELECT IIF(exists(SELECT 1 FROM [User] WHERE {argName} = @{argName}), 1, 0)
		""";
		
		var parameters = new DynamicParameters();
		parameters.Add(argName, arg);
		
		var res = await _db.LoadScalar<bool>(sql, parameters, ct);
		
		return res;
	}

	public async Task SaveAsync(User user, CancellationToken ct)
	{
		const string sql = """
			INSERT INTO [User] (id, username, email, password, created_at, updated_at, role)
			VALUES (@id, @username, @email, @password, @createdAt, @updatedAt, @role)
		""";
		
		var parameters= new DynamicParameters(user);
		
		await _db.SaveData(sql, parameters, ct);
	}
	
	public async Task<User?> GetAsync<T>(T arg, Column column, CancellationToken ct = default)
	{
		var argName = GetColumnName(column);
		
		var sql = $"""
			SELECT id, username, email, id, username, email, password, created_at createdAt, updated_at updatedAt, role
			FROM [User] WHERE {argName} = @{argName}
		""";
		
		var parameters = new DynamicParameters();
		parameters.Add(argName, arg);
		
		return await _db.LoadFirst<User>(sql, parameters, ct);
	}
	
	public async Task<string> GetEmailAsync(Guid userId, CancellationToken ct = default)
	{
		const string sql = """
			SELECT email FROM [User] WHERE id = @userId
		""";
		
		var parameters = new DynamicParameters(new { userId });
		
		return await _db.LoadScalar<string>(sql, parameters, ct);
	}
	
	
	private static string GetColumnName(Column column) => column switch
	{
		Column.Id => "id",
		Column.Email => "email",
		Column.Username => "username",
		_ => ""
	};
}