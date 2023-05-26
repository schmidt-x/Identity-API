using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IdentityApi.Data.DataAccess;
using IdentityApi.Models;

namespace IdentityApi.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
	private readonly ISqlDataAccess _db;

	public RefreshTokenRepository(ISqlDataAccess db)
	{
		_db = db;
	}

	public async Task SaveAsync(RefreshToken refreshToken, CancellationToken ct)
	{
		var sql = """
 			INSERT INTO RefreshToken (id, jti, created_at, expires_at, used, invalidated, user_id) 
 			VALUES (@id, @jti, @createdAt, @expiresAt, @used, @invalidated, @userId)
 		""";
		
		var parameters = new DynamicParameters(refreshToken);
		
		await _db.SaveData(sql, parameters, ct);
	}

	public async Task<RefreshToken?> GetAsync(Guid tokenId, CancellationToken ct)
	{
		var sql = """
 			SELECT id, jti, created_at createdAt, expires_at expiresAt, used, invalidated, user_id userId
 			FROM RefreshToken WHERE id = @tokenId
 		""";
		
		var parameters = new DynamicParameters(new { tokenId });
		
		return await _db.LoadSingle<RefreshToken>(sql, parameters, ct);
	}

	public async Task SetUsedAsync(Guid tokenId, CancellationToken ct)
	{
		var sql = """
 			UPDATE RefreshToken SET used = 1 WHERE id = @tokenId
 		""";
		
		var parameters = new DynamicParameters(new { tokenId });
		
		await _db.SaveData(sql, parameters, ct);
	}

	public async Task InvalidateAsync(Guid tokenId, CancellationToken ct)
	{
		var sql = """
 			UPDATE RefreshToken SET invalidated = 1 WHERE id = @tokenId
 		""";
 		
 		var parameters = new DynamicParameters(new { tokenId });
 		
		await _db.SaveData(sql, parameters, ct);
	}

	public async Task InvalidateAllAsync(Guid userId, CancellationToken ct)
	{
		var sql = """
 			UPDATE RefreshToken SET invalidated = 1 WHERE user_id = @userId
 		""";
		
 		var parameters = new DynamicParameters(new { userId });
		
		await _db.SaveData(sql, parameters, ct);
	}
}