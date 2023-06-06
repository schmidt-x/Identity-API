using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IdentityApi.Data.DataAccess;
using IdentityApi.Models;

namespace IdentityApi.Data.Repositories;

public class UserRepository : IUserRepository
{
	private readonly ISqlDataAccess _db;
	
	public UserRepository(ISqlDataAccess db)
	{
		_db = db;
	}
	
	
	public Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
	{
		const string sql = "SELECT IIF(exists(SELECT 1 FROM [User] WHERE email = @email), 1, 0)";
		
		var parameters = new DynamicParameters(new { email });
		
		return _db.LoadScalar<bool>(sql, parameters, ct);
	}
	
	public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
	{
		const string sql = "SELECT IIF(exists(SELECT 1 FROM [User] WHERE username = @username), 1, 0)";
		
		var parameters = new DynamicParameters(new { username });
		
		return _db.LoadScalar<bool>(sql, parameters, ct);
	}

	public Task SaveAsync(User user, CancellationToken ct = default)
	{
		const string sql = """
			INSERT INTO [User] (id, username, email, password_hash, created_at, updated_at, role)
			VALUES (@id, @username, @email, @passwordHash, @createdAt, @updatedAt, @role)
		""";
		
		var parameters= new DynamicParameters(user);
		
		return _db.SaveData(sql, parameters, ct);
	}
	
	public Task<User?> GetAsync(string email, CancellationToken ct = default)
	{
		const string sql = "SELECT * FROM [User] WHERE email = @email";
		
		var parameters = new DynamicParameters(new { email });
		
		return _db.LoadSingle<User>(sql, parameters, ct);
	}
	
	public Task<User?> GetAsync(Guid id, CancellationToken ct = default)
	{
		const string sql = "SELECT * FROM [User] WHERE id = @id";
		
		var parameters = new DynamicParameters(new { id });
		
		return _db.LoadSingle<User>(sql, parameters, ct);
	}

	public Task<string?> GetRoleAsync(Guid id, CancellationToken ct = default)
	{
		const string sql = "SELECT role FROM [User] WHERE id = @id";
		
		var parameters = new DynamicParameters(new { id });
		
		return _db.LoadScalar<string>(sql, parameters, ct);
	}
}