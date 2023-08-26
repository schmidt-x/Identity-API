using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using IdentityApi.Data.DataAccess;
using IdentityApi.Domain.Models;

namespace IdentityApi.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
	private readonly ISqlDataAccess _db;

	public RefreshTokenRepository(ISqlDataAccess db)
	{
		_db = db;
	}


	public Task SaveAsync(RefreshToken refreshToken, CancellationToken ct)
	{
		const string sql = """
 			INSERT INTO RefreshToken (id, jti, created_at, expires_at, used, invalidated, user_id) 
 			VALUES (@id, @jti, @createdAt, @expiresAt, @used, @invalidated, @userId)
 		""";
		
		var parameters = new DynamicParameters(refreshToken);
		
		return _db.ExecuteAsync(sql, parameters, ct);
	}

	public Task<RefreshToken?> GetAsync(Guid tokenId, CancellationToken ct)
	{
		const string sql = "SELECT * FROM RefreshToken WHERE id = @tokenId";
		
		var parameters = new DynamicParameters(new { tokenId });
		
		return _db.LoadSingleOrDefaultAsync<RefreshToken>(sql, parameters, ct);
	}

	public Task<Guid> SetUsedAsync(Guid tokenId, CancellationToken ct)
	{
		const string sql = """
			UPDATE [RefreshToken] 
			SET used = 1
			OUTPUT INSERTED.jti
			WHERE id = @tokenId
			""";
		
		var parameters = new DynamicParameters(new { tokenId });
		
		return _db.LoadSingleAsync<Guid>(sql, parameters, ct);
	}

	public Task<IEnumerable<Guid>> InvalidateAllAsync(Guid userId, CancellationToken ct)
	{
		const string sql = """
			SELECT jti FROM RefreshToken WHERE user_id = @userId and invalidated = 0 and used = 0
			UPDATE RefreshToken SET invalidated = 1 WHERE user_id = @userId
		""";
		
 		var parameters = new DynamicParameters(new { userId });
		
		return _db.LoadAsync<Guid>(sql, parameters, ct);
	}
	
	public Task UpdateJtiAsync(Guid jti, Guid newJti, CancellationToken ct)
	{
		const string sql = "UPDATE RefreshToken SET jti = @newJti WHERE jti = @jti";
		
		var parameters = new DynamicParameters(new { jti, newJti });
		
		return _db.ExecuteAsync(sql, parameters, ct);
	}
	
	public Task UpdateJtiAndSetValidAsync(Guid jti, Guid newJti, CancellationToken ct)
	{
		const string sql = """
			UPDATE RefreshToken
			SET jti = @newJti, invalidated = 0
			WHERE jti = @jti
		""";
		
		var parameters = new DynamicParameters(new { jti, newJti });
		
		return _db.ExecuteAsync(sql, parameters, ct);
	}
}