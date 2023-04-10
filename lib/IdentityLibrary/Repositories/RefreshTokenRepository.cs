namespace IdentityLibrary.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
	private readonly IConfiguration _config;
	private IDbConnection CreateConnection() => new SqlConnection(_config.GetConnectionString("Default"));
	
	public RefreshTokenRepository(IConfiguration config)
	{
		_config = config;
	}
	
	
	public async Task SaveAsync(RefreshToken refreshToken)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			INSERT INTO RefreshToken (id, jti, created_at, expires_at, used, invalidated, user_id) 
			VALUES (@id, @jti, @createdAt, @expiresAt, @used, @invalidated, @userId)
		""";
		
		await cnn.ExecuteAsync(sql, refreshToken);
	}

	public async Task<RefreshToken?> GetAsync(Guid tokenId)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			SELECT * FROM RefreshToken WHERE id = @id
		""";
		
		return await cnn.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { id = tokenId });
	}

	public Task SetUsedAsync(Guid tokenId)
	{
		var sql = """
			UPDATE RefreshToken SET used = 1 WHERE id = @arg
		""";
		
		return ExecuteAsync(sql, tokenId);
	}

	public Task InvalidateAsync(Guid tokenId)
	{
		var sql = """
			UPDATE RefreshToken SET invalidated = 1 WHERE id = @arg
		""";
		
		return ExecuteAsync(sql, tokenId);
	}
	
	public Task InvalidateAllAsync(Guid userId)
	{
		var sql = """
			UPDATE RefreshToken SET invalidated = 1 WHERE user_id = @arg
		""";
		
		return ExecuteAsync(sql, userId);
	}
	
	private async Task ExecuteAsync<T>(string sql, T arg)
	{
		using var cnn = CreateConnection();
		
		await cnn.ExecuteAsync(sql, new { arg });
	}
}