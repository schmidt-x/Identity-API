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
	
	
	public async Task SaveAsync(User user)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			INSERT INTO [User] (id, username, email, password, created_at, role)
			VALUES (@id, @username, @email, @password, @createdAt, @role)
		""";
		
		await cnn.ExecuteAsync(sql, user);
	}

	public async Task<ExistsResult> ExistsAsync(string username, string email)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			SELECT 1 FROM [User] WHERE username = @username
			SELECT 1 FROM [User] WHERE email = @email
		""";
		
		using var multi = await cnn.QueryMultipleAsync(sql, new { username, email });
		var usernameExists = multi.ReadFirstOrDefault<bool>();
		var emailExists = multi.ReadFirstOrDefault<bool>();
		
		if (!usernameExists && !emailExists) 
			return new() { Exists = false };
		
		var result = new ExistsResult { Exists = true, Errors = new() };
		
		if (usernameExists)
			result.Errors.Add("username", "Username is already taken");
			 
		if (emailExists)
			result.Errors.Add("email", "Email is already used");
			
		return result;
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