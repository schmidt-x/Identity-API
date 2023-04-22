namespace IdentityLibrary.Repositories;

public class UserRepository : IUserRepository
{
	private readonly DbConfig _config;
	
	private IDbConnection CreateConnection() => 
		new SqlConnection(_config.ConnectionString);
	
	public UserRepository(DbConfig config)
	{
		_config = config;
	}
	
	
	public async Task<bool> EmailExistsAsync(string email)
	{
		using var cnn = CreateConnection();
		
		const string sql = """
			SELECT 1 FROM [User] WHERE email = @email
		""";
		
		return await cnn.QueryFirstOrDefaultAsync<bool>(sql, new { email });
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

	public async Task<bool> UsernameExistsAsync(string username)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			SELECT 1 FROM [User] WHERE username = @username
		""";
		
		return await cnn.QueryFirstOrDefaultAsync<bool>(sql, new { username });
	}

	public async Task<User?> GetAsync(string username)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			SELECT * FROM [User] WHERE username = @username
		""";
		
		return await cnn.QueryFirstOrDefaultAsync<User>(sql, new { username });
	}

	public async Task<UserClaims?> GetClaims(Guid userId)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			SELECT id, username, email FROM [User] WHERE id = @userId
		""";
		
		return await cnn.QueryFirstOrDefaultAsync<UserClaims>(sql, new { userId });
	}
}