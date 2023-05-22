using System;
using System.Collections.Generic;
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
		var argName = column switch
		{
			Column.Email => "email",
			Column.Username => "username",
			_ => ""
		};
		
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
		
		var param = new DynamicParameters(user); // check if it works
		
		var parameters = new DynamicParameters(new
		{
			user.Id,
			user.Username,
			user.Email,
			user.Password,
			user.CreatedAt,
			user.UpdatedAt,
			user.Role
		});
		
		await _db.SaveData(sql, parameters, ct);
	}

	public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
	{
		throw new NotImplementedException();
	}

	public Task<UserClaims?> GetClaimsAsync(Guid userId, CancellationToken ct)
	{
		throw new NotImplementedException();
	}
}