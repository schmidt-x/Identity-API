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
	
	public async Task<UserExistsResult> UserExistsAsync(string email, string username)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			IF EXISTS (SELECT 1 FROM [User] WHERE email = @email)
				SELECT 1
			ELSE
				SELECT 0

			IF EXISTS (SELECT 1 FROM [User] WHERE username = @username)
				SELECT 1
			ELSE
				SELECT 0
		""";
		
		using var multi = await cnn.QueryMultipleAsync(sql, new { username, email });
		
		var emailExists = multi.ReadFirst<bool>();
		var usernameExists = multi.ReadFirst<bool>();
		
		if (!usernameExists && !emailExists)
		{
			return new UserExistsResult { Exists = false };
		}
		
		var errors = new Dictionary<string, IEnumerable<string>>();
		
		if (emailExists)
		{
			errors.Add("email", new[] { $"Email address '{email}' is already taken" });
		}
		
		if (usernameExists)
		{
			errors.Add("username", new[] { $"Username '{username}' is already taken" });
		}
		
		
		return new UserExistsResult { Exists = true, Errors = errors };
	}

	public async Task<User?> GetByEmailAsync(string email)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			SELECT * FROM [User] WHERE email = @email
		""";
		
		return await cnn.QueryFirstOrDefaultAsync<User>(sql, new { email });
	}

	public async Task<UserClaims?> GetClaims(Guid userId)
	{
		using var cnn = CreateConnection();
		
		var sql = """
			SELECT id, username, email FROM [User] WHERE id = @userId
		""";
		
		return await cnn.QueryFirstOrDefaultAsync<UserClaims>(sql, new { userId });
	} // TODO do I need it anymore?
}