﻿using System;
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
	
	
	public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
	{
		const string sql = """
			SELECT IIF(exists(SELECT 1 FROM [User] WHERE email = @email), 1, 0)
		""";
		
		var parameters = new DynamicParameters(new { email });
		
		return await _db.LoadScalar<bool>(sql, parameters, ct);
	}
	
	public async Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
	{
		const string sql = """
			SELECT IIF(exists(SELECT 1 FROM [User] WHERE username = @username), 1, 0)
		""";
		
		var parameters = new DynamicParameters(new { username });
		
		return await _db.LoadScalar<bool>(sql, parameters, ct);
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
	
	public async Task<User?> GetAsync(string email, CancellationToken ct = default)
	{
		const string sql = """
			SELECT id, username, email, id, username, email, password, created_at createdAt, updated_at updatedAt, role
			FROM [User] WHERE email = @email
		""";
		
		var parameters = new DynamicParameters(new { email });
		
		return await _db.LoadSingle<User>(sql, parameters, ct);
	}
	
	public async Task<User?> GetAsync(Guid id, CancellationToken ct = default)
	{
		const string sql = """
			SELECT id, username, email, id, username, email, password, created_at createdAt, updated_at updatedAt, role
			FROM [User] WHERE id = @id
		""";
		
		var parameters = new DynamicParameters(new { id });
		
		return await _db.LoadSingle<User>(sql, parameters, ct);
	}

	public async Task<string> GetRoleAsync(Guid id, CancellationToken ct = default)
	{
		const string sql = """
			SELECT role FROM [User] WHERE id = @id
		""";
		
		var parameters = new DynamicParameters(new { id });
		
		return await _db.LoadScalar<string>(sql, parameters, ct);
	}
}