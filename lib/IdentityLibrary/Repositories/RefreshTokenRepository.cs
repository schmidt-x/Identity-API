namespace IdentityLibrary.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
	private readonly IConfiguration _config;
	private IDbConnection CreateConnection() => new SqlConnection(_config.GetConnectionString("Default"));
	
	public RefreshTokenRepository(IConfiguration config)
	{
		_config = config;
	}
	
	
	public Task SaveAsync(RefreshToken refreshToken)
	{
		
		
		throw new NotImplementedException();
	}

	public Task<RefreshToken?> GetAsync(Guid id)
	{
		using var cnn = CreateConnection();
		
		
		throw new NotImplementedException();
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