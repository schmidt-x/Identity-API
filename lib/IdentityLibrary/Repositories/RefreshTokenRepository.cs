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

	public async Task<RefreshToken?> GetAsync(string id)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			SELECT * FROM RefreshToken WHERE id = @id
		""";
		
		return await cnn.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { id });
	}

	public Task SetUsedAsync(Guid id)
	{
		throw new NotImplementedException();
	}

	public Task InvalidateAsync(Guid id)
	{
		throw new NotImplementedException();
	}
}