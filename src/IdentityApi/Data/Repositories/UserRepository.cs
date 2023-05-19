using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IdentityApi.Data.DataAccess;
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
	
	public async Task<bool> EmailExistsAsync(string email, CancellationToken ct)
	{
		const string sql = """
 			SELECT IIF(exists(SELECT 1 FROM [User] WHERE email = @email), 1, 0)
 		""";
 		
 		var parameters = new DynamicParameters(new { email });
 		
 		return await _db.LoadScalar<bool>(sql, parameters, ct);
	}

	public Task<bool> UsernameExistsAsync(string username, CancellationToken ct)
	{
		throw new NotImplementedException();
	}

	public async Task<UserExistsResult> UserExistsAsync(string email, string username, CancellationToken ct)
	{
		const string sql = """
			SELECT IIF(exists(SELECT 1 FROM [User] WHERE email = @email), 1, 0)
			SELECT IIF(exists(SELECT 1 FROM [User] WHERE username = @username), 1, 0)
		""";
		
		var parameters = new DynamicParameters(new { email, username });
		
		using var multi = await _db.GetMulti(sql, parameters, ct);
		
		var emailExists = multi.ReadFirst<bool>(); // Exception
		var usernameExists = multi.ReadFirst<bool>();
		
		if (!usernameExists && !emailExists)
		{
			return new UserExistsResult { Exists = false };
		}
		
		var errors = new Dictionary<string, IEnumerable<string>>();
		
		if (emailExists)
		{
			errors.Add("email", new[] { $"Email address '{email}' is already taken" });
		}
		
		if (usernameExists)
		{
			errors.Add("username", new[] { $"Username '{username}' is already taken" });
		}
		
		return new UserExistsResult { Exists = true, Errors = errors };
	}

	public Task SaveAsync(User user, CancellationToken ct)
	{
		throw new NotImplementedException();
	}

	public Task<User?> GetByEmailAsync(string email, CancellationToken ct)
	{
		throw new NotImplementedException();
	}

	public Task<UserClaims?> GetClaims(Guid userId, CancellationToken ct)
	{
		throw new NotImplementedException();
	}
}