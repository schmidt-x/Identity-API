using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IdentityApi.Data.DataAccess;
using IdentityApi.Domain.Models;

namespace IdentityApi.Data.Repositories;

public class UserRepository : IUserRepository
{
	private readonly ISqlDataAccess _db;
	
	public UserRepository(ISqlDataAccess db)
	{
		_db = db;
	}
	
	
	public Task<bool> EmailExistsAsync(string email, CancellationToken ct)
	{
		const string sql = "SELECT IIF(exists(SELECT 1 FROM [User] WHERE email = @email), 1, 0)";
		
		var parameters = new DynamicParameters(new { email });
		
		return _db.LoadScalarAsync<bool>(sql, parameters, ct);
	}
	
	public Task<bool> UsernameExistsAsync(string username, CancellationToken ct)
	{
		const string sql = "SELECT IIF(exists(SELECT 1 FROM [User] WHERE username = @username), 1, 0)";
		
		var parameters = new DynamicParameters(new { username });
		
		return _db.LoadScalarAsync<bool>(sql, parameters, ct);
	}

	public Task SaveAsync(User user, CancellationToken ct)
	{
		const string sql = """
			INSERT INTO [User] (id, username, email, password_hash, created_at, updated_at, role)
			VALUES (@id, @username, @email, @passwordHash, @createdAt, @updatedAt, @role)
		""";
		
		var parameters= new DynamicParameters(user);
		
		return _db.ExecuteAsync(sql, parameters, ct);
	}
	
	public Task<User?> GetAsync(string email, CancellationToken ct)
	{
		const string sql = "SELECT * FROM [User] WHERE email = @email";
		
		var parameters = new DynamicParameters(new { email });
		
		return _db.LoadSingleOrDefaultAsync<User>(sql, parameters, ct);
	}
	
	public Task<User?> GetAsync(Guid id, CancellationToken ct)
	{
		const string sql = "SELECT * FROM [User] WHERE id = @id";
		
		var parameters = new DynamicParameters(new { id });
		
		return _db.LoadSingleOrDefaultAsync<User>(sql, parameters, ct);
	}
	
	public Task<User> GetRequiredAsync(Guid id, CancellationToken ct)
	{
		const string sql = "SELECT * FROM [User] WHERE id = @id";
		
		var parameters = new DynamicParameters(new { id });
		
		return _db.LoadSingleAsync<User>(sql, parameters, ct);
	}
	
	public Task<UserProfile> GetProfileAsync(Guid id, CancellationToken ct)
	{
		const string sql = """
			SELECT username, email, created_at, updated_at, role
			FROM [User] WHERE id = @id
		""";
		
		var parameters = new DynamicParameters(new { id });
		
		return _db.LoadSingleAsync<UserProfile>(sql, parameters, ct);
	}

	public Task<string> GetRoleAsync(Guid id, CancellationToken ct)
	{
		const string sql = "SELECT role FROM [User] WHERE id = @id";
		
		var parameters = new DynamicParameters(new { id });
		
		return _db.LoadScalarAsync<string>(sql, parameters, ct);
	}
	
	public Task<string> GetPasswordHashAsync(Guid id, CancellationToken ct)
	{
		const string sql = "SELECT password_hash FROM [User] WHERE id = @id";
		
		var parameters = new DynamicParameters(new { id });
		
		return _db.LoadScalarAsync<string>(sql, parameters, ct);
	}
	
	public Task<UserProfile> UpdateUsernameAsync(Guid id, string username, CancellationToken ct)
	{
		const string sql = """
			UPDATE [User]
			SET username = @username, updated_at = GETUTCDATE()
			WHERE id = @id
			
			SELECT username, email, created_at, updated_at, role
			FROM [User] WHERE id = @id
		""";
		
		var parameters = new DynamicParameters(new { id, username });
		
		return _db.LoadSingleAsync<UserProfile>(sql, parameters, ct);
	}

	public Task<UserProfile> UpdateEmailAsync(Guid id, string email, CancellationToken ct)
	{
		const string sql = """
			UPDATE [User]
			SET email = @email, updated_at = GETUTCDATE()
			WHERE id = @id
			
			SELECT username, email, created_at, updated_at, role
			FROM [User] WHERE id = @id
		""";
		
		var parameters = new DynamicParameters(new { id, email });
		
		return _db.LoadSingleAsync<UserProfile>(sql, parameters, ct);
	}
	
	public Task<UserProfile> UpdatePasswordAsync(Guid id, string password, CancellationToken ct)
	{
		const string sql = """
			UPDATE [User] 
			SET password_hash = @password, updated_at = GETUTCDATE() 
			WHERE id = @id
			
			SELECT username, email, created_at, updated_at, role
			FROM [User] WHERE id = @id
		""";
		
		var parameters = new DynamicParameters(new { id, password });
		
		return _db.LoadSingleAsync<UserProfile>(sql, parameters, ct);
	}
	
}