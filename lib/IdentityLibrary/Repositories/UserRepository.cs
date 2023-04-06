namespace IdentityLibrary.Repositories;

public class UserRepository : IUserRepository
{
	private readonly IConfiguration _config;
	
	private IDbConnection CreateConnection() => 
		new SqlConnection(_config.GetConnectionString("Default"));
	
	public UserRepository(IConfiguration config)
	{
		_config = config;
	}
	
	
	public Task SaveAsync(User user)
	{
		throw new NotImplementedException();
	}

	public Task<ExistsResult> ExistsAsync(string username, string email)
	{
		throw new NotImplementedException();
	}

	public Task<User?> GetAsync(string username)
	{
		throw new NotImplementedException();
	}

	public Task<UserClaims?> GetClaims(Guid id)
	{
		throw new NotImplementedException();
	}
}